namespace XIVExtraComboVX.GameData;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Dalamud.Hooking;
using Dalamud.Logging;

using XIVExtraComboVX.Combos;

internal class IconReplacer: IDisposable {

	private delegate ulong IsIconReplaceableDelegate(uint actionID);
	private delegate uint GetIconDelegate(IntPtr actionManager, uint actionID);
	private delegate IntPtr GetActionCooldownSlotDelegate(IntPtr actionManager, int cooldownGroup);

	private readonly Hook<IsIconReplaceableDelegate> isIconReplaceableHook;
	private readonly Hook<GetIconDelegate> getIconHook;

	private IntPtr actionManager = IntPtr.Zero;

	private readonly List<CustomCombo> customCombos;

	public IconReplacer() {
		PluginLog.Information("Loading registered combos");
		this.customCombos = Assembly.GetAssembly(this.GetType())!.GetTypes()
			.Where(t => !t.IsAbstract && (t.BaseType == typeof(CustomCombo) || t.BaseType?.BaseType == typeof(CustomCombo)))
			.Select(t => Activator.CreateInstance(t))
			.Cast<CustomCombo>()
			.ToList();
		PluginLog.Information($"Loaded {this.customCombos.Count} replacers");
#if DEBUG
		PluginLog.Information(string.Join(", ", this.customCombos.Select(combo => combo.GetType().Name)));
#endif

		this.getIconHook = Hook<GetIconDelegate>.FromAddress(Service.Address.GetAdjustedActionId, this.getIconDetour);
		this.isIconReplaceableHook = Hook<IsIconReplaceableDelegate>.FromAddress(Service.Address.IsActionIdReplaceable, this.isIconReplaceableDetour);

		this.getIconHook.Enable();
		this.isIconReplaceableHook.Enable();

	}

	public void Dispose() {
		this.getIconHook?.Disable();
		this.isIconReplaceableHook?.Disable();

		this.getIconHook?.Dispose();
		this.isIconReplaceableHook?.Dispose();
	}

	private ulong isIconReplaceableDetour(uint actionID) => 1;

	/// <summary>
	/// Replace an ability with another ability
	/// actionID is the original ability to be "used"
	/// Return either actionID (itself) or a new Action table ID as the
	/// ability to take its place.
	/// I tend to make the "combo chain" button be the last move in the combo
	/// For example, Souleater combo on DRK happens by dragging Souleater
	/// onto your bar and mashing it.
	/// </summary>
	private unsafe uint getIconDetour(IntPtr actionManager, uint actionID) {
		try {
			this.actionManager = actionManager;

			if (Service.Client.LocalPlayer is null)
				return this.OriginalHook(actionID);

			uint following = *(uint*)Service.Address.LastComboMove;
			float time = *(float*)Service.Address.ComboTimer;
			byte level = Service.Client.LocalPlayer?.Level ?? 0;

			foreach (CustomCombo combo in this.customCombos) {
				if (combo.TryInvoke(actionID, following, time, level, out uint newActionID))
					return newActionID;
			}

			return this.OriginalHook(actionID);
		}
		catch (Exception ex) {
			Service.Logger.error("Don't crash the game", ex);
			return this.getIconHook.Original(actionManager, actionID);
		}
	}


	public uint OriginalHook(uint actionID) => this.getIconHook.Original(this.actionManager, actionID);

}
