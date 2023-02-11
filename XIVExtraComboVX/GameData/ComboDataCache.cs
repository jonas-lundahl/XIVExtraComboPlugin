namespace XIVExtraComboVX.GameData;

using System;
using System.Collections.Generic;

using Dalamud.Game;
using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.ClientState.Statuses;

using XIVExtraComboVX.Combos;

internal class ComboDataCache: ManagedCache {
	protected const uint InvalidObjectID = 0xE000_0000;

	// Invalidate these
	private readonly Dictionary<(uint StatusID, uint? TargetID, uint? SourceID), Status?> statusCache = new();
	private readonly Dictionary<uint, CooldownData> cooldownCache = new();
	private bool? canInterruptTarget = null;
	private uint? dancerNextDanceStep = null;

	// Do not invalidate these
	private readonly Dictionary<uint, byte> cooldownGroupCache = new();
	private readonly Dictionary<Type, JobGaugeBase> jobGaugeCache = new();
	private readonly Dictionary<(uint ActionID, uint ClassJobID, byte Level), (ushort CurrentMax, ushort Max)> chargesCache = new();

	#region Core/setup

	private delegate IntPtr GetActionCooldownSlotDelegate(IntPtr actionManager, int cooldownGroup);

	public ComboDataCache() : base() { }

	protected override void Dispose(bool disposing) {
		base.Dispose(disposing);
		this.jobGaugeCache?.Clear();
	}

	protected override unsafe void InvalidateCache(Framework framework) {
		this.statusCache.Clear();
		this.cooldownCache.Clear();
		this.canInterruptTarget = null;
		this.dancerNextDanceStep = null;
	}

	#endregion

	public bool CanInterruptTarget {
		get {
			if (!this.canInterruptTarget.HasValue) {
				GameObject? target = Service.Targets.Target;
				this.canInterruptTarget = target is not null
					&& target is BattleChara actor
					&& actor.IsCasting
					&& actor.IsCastInterruptible;
			}
			return this.canInterruptTarget.Value;
		}
	}

	public bool DancerSmartDancing(out uint nextStep) {
		this.dancerNextDanceStep ??= CustomCombo.DancerDancing();

		nextStep = this.dancerNextDanceStep.Value;

		return nextStep > 0;
	}

	public T GetJobGauge<T>() where T : JobGaugeBase {
		if (!this.jobGaugeCache.TryGetValue(typeof(T), out JobGaugeBase? gauge))
			gauge = this.jobGaugeCache[typeof(T)] = Service.JobGauge.Get<T>();

		return (T)gauge;
	}

	public Status? GetStatus(uint statusID, GameObject? actor, uint? sourceID) {
		(uint statusID, uint? ObjectId, uint? sourceID) key = (statusID, actor?.ObjectId, sourceID);
		if (this.statusCache.TryGetValue(key, out Status? found))
			return found;

		if (actor is null)
			return this.statusCache[key] = null;

		if (actor is not BattleChara chara)
			return this.statusCache[key] = null;

		foreach (Status? status in chara.StatusList) {
			if (status.StatusId == statusID && (!sourceID.HasValue || status.SourceId == 0 || status.SourceId == InvalidObjectID || status.SourceId == sourceID))
				return this.statusCache[key] = status;
		}

		return this.statusCache[key] = null;
	}

	public unsafe CooldownData GetCooldown(uint actionID) {
		if (this.cooldownCache.TryGetValue(actionID, out CooldownData found))
			return found;

		FFXIVClientStructs.FFXIV.Client.Game.ActionManager* actionManager = FFXIVClientStructs.FFXIV.Client.Game.ActionManager.Instance();
		if (actionManager is null)
			return this.cooldownCache[actionID] = default;

		byte cooldownGroup = this.getCooldownGroup(actionID);

		FFXIVClientStructs.FFXIV.Client.Game.RecastDetail* cooldownPtr = actionManager->GetRecastGroupDetail(cooldownGroup - 1);
		cooldownPtr->ActionID = actionID;

		CooldownData cd = this.cooldownCache[actionID] = *(CooldownData*)cooldownPtr;
		Service.Logger.debug($"Retrieved cooldown data for action #{actionID}: {cd.DebugLabel}");
		return cd;
	}

	public unsafe (ushort Current, ushort Max) GetMaxCharges(uint actionID) {
		PlayerCharacter player = Service.Client.LocalPlayer!;
		if (player == null)
			return (0, 0);

		uint job = player.ClassJob.Id;
		byte level = player.Level;
		if (job == 0 || level == 0)
			return (0, 0);

		(uint actionID, uint job, byte level) key = (actionID, job, level);
		if (this.chargesCache.TryGetValue(key, out (ushort CurrentMax, ushort Max) found))
			return found;

		ushort cur = FFXIVClientStructs.FFXIV.Client.Game.ActionManager.GetMaxCharges(actionID, 0);
		ushort max = FFXIVClientStructs.FFXIV.Client.Game.ActionManager.GetMaxCharges(actionID, 90);
		return this.chargesCache[key] = (cur, max);
	}

	private byte getCooldownGroup(uint actionID) {
		if (this.cooldownGroupCache.TryGetValue(actionID, out byte cooldownGroup))
			return cooldownGroup;

		Lumina.Excel.ExcelSheet<Lumina.Excel.GeneratedSheets.Action> sheet = Service.GameData.GetExcelSheet<Lumina.Excel.GeneratedSheets.Action>()!;
		Lumina.Excel.GeneratedSheets.Action row = sheet.GetRow(actionID)!;

		return this.cooldownGroupCache[actionID] = row.CooldownGroup;
	}

}
