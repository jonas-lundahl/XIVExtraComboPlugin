namespace XIVExtraComboVX.Config;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Interface.Windowing;
using Dalamud.Logging;
using Dalamud.Utility;

using ImGuiNET;

using XIVExtraComboVX.Attributes;

public class ConfigWindow: Window {

	private readonly Dictionary<string, List<(CustomComboPreset preset, CustomComboInfoAttribute info)>> groupedPresets;
	private readonly Dictionary<CustomComboPreset, List<(CustomComboPreset Preset, CustomComboInfoAttribute Info)>> parentToChildrenPresets = new();
	private readonly Dictionary<CustomComboPreset, (CustomComboPreset Preset, CustomComboInfoAttribute Info)> childToParentPresets = new();
	private readonly Dictionary<CustomComboPreset, int> presetOrdinals = new();
	private readonly Dictionary<CustomComboPreset, List<ComboDetailSetting>> detailSettings = new();

	private readonly string[] sortedJobs;

	private static readonly Vector4 shadedColour = new(0.69f, 0.69f, 0.69f, 1f); // NICE (x3 COMBO)
	private static readonly Vector4 activeColour = new(0f, 139f / 255f, 69f / 255f, 1f);
	private static readonly Vector4 warningColour = new(200f / 255f, 25f / 255f, 35f / 255f, 1f);
	private static readonly Vector4 deprecatedColour = new(0f / 255f, 95f / 255f, 190f / 255f, 1f);

	private const int minWidth = 900;

	public ConfigWindow() : base($"Custom Combo Setup - {Service.Plugin.ShortPluginSignature}, {Service.Plugin.PluginBuildType}###{Service.Plugin.Name} Custom Combo Setup", ImGuiWindowFlags.MenuBar) {
		this.RespectCloseHotkey = true;

#pragma warning disable CS8619 // Nullability of reference types in value doesn't match target type.
		List<(CustomComboPreset preset, CustomComboInfoAttribute info)> realPresets = Enum
			.GetValues<CustomComboPreset>()
			.Where(preset => (int)preset >= 100)
			.Select(preset => (
				preset,
				info: preset.GetAttribute<CustomComboInfoAttribute>()
			))
			.Where(preset => preset.info is not null) // warning is voided by this line - the CCIA will never be null
			.OrderBy(preset => preset.info!.Order)
			.ToList();
#pragma warning restore CS8619 // Nullability of reference types in value doesn't match target type.

		this.groupedPresets = realPresets
			.GroupBy(data => data.info.JobName)
			.OrderBy(group => group.Key)
			.ToDictionary(
				group => group.Key,
				data => data
					.OrderBy(e => e.info.Order)
					.ToList()
			);

		this.detailSettings = typeof(PluginConfiguration)
			.GetProperties(BindingFlags.Instance | BindingFlags.Public)
			.Select(prop => (prop, attr: prop.GetCustomAttribute<ComboDetailSettingAttribute>()))
			.Where(pair => pair.attr is not null)
			.Select(pair => new ComboDetailSetting(pair.prop, pair.attr!))
			.GroupBy(detail => detail.Combo)
			.ToDictionary(
				group => group.Key,
				group => group
					.OrderBy(detail => detail.Label)
					.ToList()
			);

		foreach ((CustomComboPreset preset, CustomComboInfoAttribute info) in realPresets) {
			CustomComboPreset? parent = preset.GetParent();
			CustomComboInfoAttribute? parentInfo = parent?.GetAttribute<CustomComboInfoAttribute>();
			if (parent is not null && parentInfo is not null) {
				this.childToParentPresets.Add(preset, (parent.Value, parentInfo));
				if (!this.parentToChildrenPresets.ContainsKey(parent.Value)) {
					this.parentToChildrenPresets[parent.Value] = new();
				}
				this.parentToChildrenPresets[parent.Value].Add((preset, info));
			}
		}

		this.sortedJobs = this.groupedPresets.Keys
			.Where(j => !j.StartsWith("Disciple of the "))
			.Concat(
				this.groupedPresets.Keys
					.Where(j => j.StartsWith("Disciple of the "))
			)
			.ToArray();

		int ord = 0;
		foreach (string job in this.sortedJobs) {
			foreach ((CustomComboPreset preset, CustomComboInfoAttribute info) in this.groupedPresets[job]) {
				if (this.childToParentPresets.ContainsKey(preset))
					continue; // if this IS a child preset, it'll be handled when we reach the parent (which may have already happened)

				this.presetOrdinals[preset] = ++ord;

				PluginLog.Information($"Indexed {preset} as {ord}");

				// if this preset has children, iterate and index them immediately because they'll be grouped under it in the config window
				if (this.parentToChildrenPresets.TryGetValue(preset, out List<(CustomComboPreset Preset, CustomComboInfoAttribute Info)>? children) && children?.Count > 0) {
					// but we can't recurse here, so we make a queue to process
					// and since we need to be tricky with the ordering, it's actually a linked list
					LinkedList<CustomComboPreset> queue = new();
					queue.AddLast(preset);

					while (queue.Count > 0) {
						LinkedListNode<CustomComboPreset>? head = queue.First;
						if (head is null)
							break;
						queue.RemoveFirst();
						CustomComboPreset next = head.Value;
						if (!this.presetOrdinals.ContainsKey(next))
							this.presetOrdinals[next] = ++ord;

						PluginLog.Information($"Indexed {next} as {ord}");

						// if the current preset being indexed has children, they need to be added to the FRONT of the queue (but still in the order they're presented)
						if (this.parentToChildrenPresets.TryGetValue(next, out List<(CustomComboPreset Preset, CustomComboInfoAttribute Info)>? subchildren)) {
							// to that end, we reverse the list of children, and then add each to the front of the list, effectively doing queue.unshift(children.pop()) until the list is emptied
							foreach ((CustomComboPreset Preset, CustomComboInfoAttribute _) in subchildren.ToArray().Reverse())
								queue.AddFirst(Preset);
						}
					}
				}
			}
		}

		this.SizeCondition = ImGuiCond.FirstUseEver;
		this.Size = new(minWidth, 800);
		this.SizeConstraints = new() {
			MinimumSize = new(minWidth, 400),
			MaximumSize = new(int.MaxValue, int.MaxValue),
		};
	}

	public override void Draw() {

		bool pluginActive = Service.Configuration.Active;
		bool hideChildren = Service.Configuration.HideDisabledFeaturesChildren;
		bool registerNormalCommand = Service.Configuration.RegisterCommonCommand;
		bool showUpdateMessage = Service.Configuration.ShowUpdateMessage;
		bool compactMode = Service.Configuration.CompactSettingsWindow;

		if (ImGui.BeginMenuBar()) {

			if (ImGui.BeginMenu("Settings")) {

				bool clickEnabled = ImGui.MenuItem("Enable action replacement", "", ref pluginActive);
				if (ImGui.IsItemHovered()) {
					ImGui.BeginTooltip();
					ImGui.Text("If disabled, no actions will be replaced, regardless of configured");
					ImGui.Text("combos and features. This allows you to see what actions you actually");
					ImGui.Text("assigned on your hotbars without manually disabling everything and");
					ImGui.Text("having to turn it all back on after, one by one.");
					ImGui.Text("");
					ImGui.Text("This option does NOT persist between plugin loads.");
					ImGui.Text("It will always be reset to ON when you launch the game.");
					ImGui.EndTooltip();
				}
				if (clickEnabled) {
					Service.Configuration.Active = pluginActive;
				}

				bool clickCollapse = ImGui.MenuItem("Collapse disabled features", "", ref hideChildren);
				if (ImGui.IsItemHovered()) {
					ImGui.BeginTooltip();
					ImGui.Text("If enabled, children of disabled features will be hidden.");
					ImGui.Text("A message will be shown under any disabled feature that");
					ImGui.Text("has child features, so you can still tell that there are more");
					ImGui.Text("features available dependent on the disabled one.");
					ImGui.EndTooltip();
				}
				if (clickCollapse) {
					Service.Configuration.HideDisabledFeaturesChildren = hideChildren;
					Service.Configuration.Save();
				}

				bool clickRegister = ImGui.MenuItem($"Register {Plugin.command}", "", ref registerNormalCommand);
				if (ImGui.IsItemHovered()) {
					ImGui.BeginTooltip();
					ImGui.Text($"If enabled, {Service.Plugin.Name} will attempt to register the {Plugin.command} command.");
					ImGui.Text("This is the command generally used by all forks of XIVCombo, which");
					ImGui.Text("means it will conflict if you have multiple forks installed. This");
					ImGui.Text("isn't advised to begin with, but this option will allow for slightly");
					ImGui.Text("better compatibility than would otherwise be available, at least.");
					ImGui.Text("");
					ImGui.Text("This plugin always registers its own (separate) command to open the");
					ImGui.Text("settings window, regardless of whether the default one is also used.");
					ImGui.Text("");
					ImGui.Text("This option only takes effect after a restart.");
					ImGui.EndTooltip();
				}
				if (clickRegister) {
					Service.Configuration.RegisterCommonCommand = registerNormalCommand;
					Service.Configuration.Save();
				}

				bool clickUpdates = ImGui.MenuItem("Show update messages", "", ref showUpdateMessage);
				if (ImGui.IsItemHovered()) {
					ImGui.BeginTooltip();
					ImGui.Text("If enabled, an alert will be shown in your chatlog whenever the plugin updates.");
					ImGui.Text("The message includes the old version, the new version, and a clickable 'link' to");
					ImGui.Text("open the plugin configuration window.");
					ImGui.EndTooltip();
				}
				if (clickUpdates) {
					Service.Configuration.ShowUpdateMessage = showUpdateMessage;
					Service.Configuration.Save();
				}

				bool clickCompact = ImGui.MenuItem("Compact display", "", ref compactMode);
				if (ImGui.IsItemHovered()) {
					ImGui.BeginTooltip();
					ImGui.Text("If enabled, combo descriptions will be moved into tooltips shown on hover.");
					ImGui.Text("This makes the combo display more compact, which can be useful with the");
					ImGui.Text("new detail settings taking up extra space.");
					ImGui.EndTooltip();
				}
				if (clickCompact) {
					Service.Configuration.CompactSettingsWindow = compactMode;
					Service.Configuration.Save();
				}

				ImGui.EndMenu();
			}

			if (ImGui.BeginMenu("Utilities")) {

				bool clickReset = ImGui.MenuItem("Reset configuration");
				if (ImGui.IsItemHovered()) {
					ImGui.BeginTooltip();
					ImGui.Text("This will completely reset your entire configuration to the defaults.");
					ImGui.TextColored(warningColour, "THIS CANNOT BE UNDONE!");
					ImGui.EndTooltip();
				}
				if (clickReset) {
					Service.Plugin.onPluginCommand("", "reset");
				}

				ImGui.EndMenu();
			}

#if DEBUG
			if (ImGui.BeginMenu("Debugging")) {

				PlayerCharacter? player = Service.Client.LocalPlayer;
				if (player is null) {
					ImGui.MenuItem("Not logged in", false);
				}
				else {
					ImGui.MenuItem($"{player.Name}: {player.ClassJob.GameData!.Abbreviation.ToString().ToUpper()} ({player.ClassJob.Id})", false);
				}

				bool clickDebug = ImGui.MenuItem("Snapshot debug messages");
				if (ImGui.IsItemHovered()) {
					ImGui.BeginTooltip();
					ImGui.Text("This enables a snapshot of debug messages in the dalamud log.");
					ImGui.Text("They will appear in your log file and also in the /xllog window.");
					ImGui.EndTooltip();
				}
				if (clickDebug) {
					Service.Logger.EnableNextTick();
				}

				ImGui.EndMenu();
			}
#endif

			ImGui.EndMenuBar();
		}

		foreach (string jobName in this.sortedJobs) {
			if (ImGui.CollapsingHeader(jobName)) {

				ImGui.PushID($"settings-{jobName}");

				foreach ((CustomComboPreset preset, CustomComboInfoAttribute info) in this.groupedPresets[jobName]) {
					if (this.childToParentPresets.ContainsKey(preset))
						continue;

					this.drawPreset(preset, info);
				}

				ImGui.PopID();

			}
		}

	}

	private void drawPreset(CustomComboPreset preset, CustomComboInfoAttribute info) {

		bool compactMode = Service.Configuration.CompactSettingsWindow;
		bool enabled = Service.Configuration.IsEnabled(preset);
		bool dangerous = preset.GetAttribute<DangerousAttribute>() is not null;
		bool experimental = preset.GetAttribute<ExperimentalAttribute>() is not null;
		bool deprecated = preset.GetAttribute<DeprecatedAttribute>() is not null;
		CustomComboPreset[] conflicts = preset.GetConflicts();
		CustomComboPreset[] alternatives = deprecated ? preset.GetAlternatives() : Array.Empty<CustomComboPreset>();
		CustomComboPreset? parent = preset.GetParent();
		bool hideChildren = Service.Configuration.HideDisabledFeaturesChildren;
		bool hasChildren = this.parentToChildrenPresets.TryGetValue(preset, out List<(CustomComboPreset Preset, CustomComboInfoAttribute Info)>? children)
			&& children is not null && children.Count > 0;
		int childCount = hasChildren ? children!.Count : 0;
		bool hasDetails = this.detailSettings.TryGetValue(preset, out List<ComboDetailSetting>? details)
			&& details is not null;
#if DEBUG
		string debugInfo = preset.GetDebugLabel();
#else
		string debugInfo = ((int)preset).ToString();
#endif

		string conflictWarning = string.Empty;
		if (conflicts.Length > 0) {
			string[] conflictNames = conflicts
				.Select(p => p.GetAttribute<CustomComboInfoAttribute>()!.FancyName)
				.ToArray();
			conflictWarning = $"Conflicts with: {string.Join(", ", conflictNames)}";
		}

		ImGui.PushItemWidth(200);
		bool toggled = ImGui.Checkbox($"{this.presetOrdinals[preset]}: {info.FancyName}", ref enabled);
		ImGui.PopItemWidth();

		if (compactMode && ImGui.IsItemHovered()) {
			ImGui.BeginTooltip();

			ImGui.TextUnformatted(info.Description);

			if (conflictWarning.Length > 0)
				ImGui.TextColored(shadedColour, conflictWarning);

			ImGui.EndTooltip();
		}

		ImGui.SameLine();
		ImGui.TextColored(shadedColour, $"[debug: {debugInfo}]");
		if (ImGui.IsItemHovered()) {
			ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
			ImGui.SetTooltip("Click to copy the debug ID for the developer");
		}
		if (ImGui.IsItemClicked())
			ImGui.SetClipboardText(debugInfo);

		if (toggled) {
			if (enabled) {

				Service.Configuration.EnabledActions.Add(preset);
				this.enableParentPresets(preset);

				foreach (CustomComboPreset conflict in conflicts) {
					Service.Configuration.EnabledActions.Remove(conflict);
				}

			}
			else {
				Service.Configuration.EnabledActions.Remove(preset);
			}

			Service.Configuration.Save();
		}

		ImGui.PushTextWrapPos((this.Size?.Y ?? minWidth) - 20);

		if (!compactMode)
			ImGui.TextUnformatted(info.Description);
		if (dangerous) {
			ImGui.TextColored(warningColour, $"UNSAFE - use {info.FancyName} at your own risk!");
			if (ImGui.IsItemHovered()) {
				ImGui.BeginTooltip();
				ImGui.TextUnformatted("Unsafe replacers use internal mechanisms that may carry a risk of crashing,"
					+ "\nespecially immediately after FFXIV or Dalamud is updated.");
				ImGui.EndTooltip();
			}
		}
		else if (experimental) {
			ImGui.TextColored(warningColour, $"EXPERIMENTAL - {info.FancyName} may change without warning!");
			if (ImGui.IsItemHovered()) {
				ImGui.BeginTooltip();
				ImGui.TextUnformatted("Experimental replacers are not fully tested, may cause unwanted or unexpected behaviour,"
					+ "\nmight not be complete, and should only be used if you accept these risks.");
				ImGui.EndTooltip();
			}
		}
		else if (deprecated) {
			ImGui.TextColored(enabled ? warningColour : deprecatedColour, $"DEPRECATED - {info.FancyName} is not recommended for use!");
			if (ImGui.IsItemHovered()) {
				ImGui.BeginTooltip();
				ImGui.TextUnformatted("Deprecated replacers are no longer being actively updated, and should be"
					+ " considered outdated. They may be removed in future versions.");
				if (alternatives.Length > 0) {
					ImGui.TextUnformatted("");
					if (alternatives.Length == 1) {
						ImGui.TextUnformatted("The developer suggests replacing this preset"
							+ (hasChildren ? " and its children" : string.Empty)
							+ $" with #{this.presetOrdinals[alternatives[0]]}: {alternatives[0].GetAttribute<CustomComboInfoAttribute>()!.FancyName}");
					}
					else {
						string initial = "The developer suggests replacing this preset"
							+ (hasChildren ? " and its children" : string.Empty)
							+ " with one or more of the following:";
						StringBuilder msg = new(initial, initial.Length + (alternatives.Length * 25));
						foreach (CustomComboPreset p in alternatives) {
							msg.Append($"\n      #{this.presetOrdinals[p]}: {p.GetAttribute<CustomComboInfoAttribute>()!.FancyName}");
						}
						ImGui.TextUnformatted(msg.ToString());
					}
				}
				ImGui.EndTooltip();
			}
		}
		if (!compactMode && conflictWarning.Length > 0)
			ImGui.TextColored(shadedColour, conflictWarning);
		if (hasChildren && hideChildren && !enabled)
			ImGui.TextColored(shadedColour, "This preset has one or more children.");
		if (hasDetails && !enabled)
			ImGui.TextColored(shadedColour, "This preset has additional configurable options.");

		ImGui.PopTextWrapPos();

		if (hasDetails && enabled) {
			const int MEM_WIDTH = sizeof(double);
			IntPtr ptrVal = Marshal.AllocHGlobal(MEM_WIDTH);
			IntPtr ptrMin = Marshal.AllocHGlobal(MEM_WIDTH);
			IntPtr ptrMax = Marshal.AllocHGlobal(MEM_WIDTH);
			IntPtr ptrStep = Marshal.AllocHGlobal(MEM_WIDTH);
			bool shift = ImGui.IsKeyDown(ImGuiKey.ModShift);
			bool ctrl = ImGui.IsKeyDown(ImGuiKey.ModCtrl);
			byte multShift = (byte)(shift ? 100 : 1);
			byte multCtrl = (byte)(ctrl ? 10 : 1);
			ushort mult = (ushort)(multShift * multCtrl);
			foreach (ComboDetailSetting? detail in details!) {
				if (detail is not null) {
					double range = detail.Max - detail.Min;
					bool useSlider = range <= 40;
					string fmt;
					switch (detail.ImGuiType) {
						case ImGuiDataType.Double:
							fmt = $"%.{detail.Precision}f";
							Marshal.Copy(BitConverter.GetBytes((double)detail.Val), 0, ptrVal, MEM_WIDTH);
							Marshal.Copy(BitConverter.GetBytes((double)detail.Min), 0, ptrMin, MEM_WIDTH);
							Marshal.Copy(BitConverter.GetBytes((double)detail.Max), 0, ptrMax, MEM_WIDTH);
							Marshal.Copy(BitConverter.GetBytes((double)mult), 0, ptrStep, MEM_WIDTH);
							break;
						case ImGuiDataType.U64:
							fmt = "%u";
							Marshal.Copy(BitConverter.GetBytes((ulong)detail.Val), 0, ptrVal, MEM_WIDTH);
							Marshal.Copy(BitConverter.GetBytes((ulong)detail.Min), 0, ptrMin, MEM_WIDTH);
							Marshal.Copy(BitConverter.GetBytes((ulong)detail.Max), 0, ptrMax, MEM_WIDTH);
							Marshal.Copy(BitConverter.GetBytes((ulong)mult), 0, ptrStep, MEM_WIDTH);
							break;
						case ImGuiDataType.S64:
							fmt = "%i";
							Marshal.Copy(BitConverter.GetBytes((long)detail.Val), 0, ptrVal, MEM_WIDTH);
							Marshal.Copy(BitConverter.GetBytes((long)detail.Min), 0, ptrMin, MEM_WIDTH);
							Marshal.Copy(BitConverter.GetBytes((long)detail.Max), 0, ptrMax, MEM_WIDTH);
							Marshal.Copy(BitConverter.GetBytes((long)mult), 0, ptrStep, MEM_WIDTH);
							break;
						default:
							throw new FormatException($"Invalid detail type {detail.ImGuiType}");
					}
					Service.Logger.debug(
						$"{detail.Label} ({detail.Type.Name}/{detail.ImGuiType}) {detail.Min} <= [{detail.Val}] <= {detail.Max} ({range})"
					);
					bool changed = useSlider
						? ImGui.SliderScalar(
							detail.Label + $"##{detail.Combo}",
							detail.ImGuiType,
							ptrVal,
							ptrMin,
							ptrMax,
							fmt,
							ImGuiSliderFlags.AlwaysClamp
						)
						: ImGui.InputScalar(
							detail.Label + $"##{detail.Combo}",
							detail.ImGuiType,
							ptrVal,
							ptrStep,
							ptrStep,
							fmt,
							ImGuiInputTextFlags.AutoSelectAll
						);
					if (ImGui.IsItemHovered()) {
						ImGui.BeginTooltip();
						ImGui.PushTextWrapPos(400);
						if (!string.IsNullOrEmpty(detail.Description)) {
							ImGui.TextUnformatted(detail.Description);
							ImGui.TextUnformatted("");
						}
						ImGui.TextUnformatted($"Range: [{detail.Min}, {detail.Max}] (inclusive)");
						if (!useSlider) {
							ImGui.PushStyleColor(ImGuiCol.Text, ctrl && !shift ? activeColour : shadedColour);
							ImGui.TextUnformatted("[CTRL] Buttons move x10");
							ImGui.PopStyleColor();
							ImGui.PushStyleColor(ImGuiCol.Text, shift && !ctrl ? activeColour : shadedColour);
							ImGui.TextUnformatted("[SHFT] Buttons move x100");
							ImGui.PopStyleColor();
							ImGui.PushStyleColor(ImGuiCol.Text, ctrl && shift ? activeColour : shadedColour);
							ImGui.TextUnformatted("[BOTH] Buttons move x1000");
							ImGui.PopStyleColor();
						}
						ImGui.PopTextWrapPos();
						ImGui.EndTooltip();
					}
					if (changed) {
						byte[] value = new byte[MEM_WIDTH];
						Marshal.Copy(ptrVal, value, 0, MEM_WIDTH);
						double val = detail.ImGuiType switch {
							ImGuiDataType.Double => BitConverter.ToDouble(value),
							ImGuiDataType.U64 => BitConverter.ToUInt64(value),
							ImGuiDataType.S64 => BitConverter.ToInt64(value),
							_ => throw new FormatException($"Invalid detail type {detail.ImGuiType}"), // theoretically unpossible
						};
						detail.Val = Math.Round(val, detail.Precision); // setter handles min/max bounding
						Service.Configuration.Save();
					}
				}
			}
			Marshal.FreeHGlobal(ptrVal);
			Marshal.FreeHGlobal(ptrMin);
			Marshal.FreeHGlobal(ptrMax);
			Marshal.FreeHGlobal(ptrStep);
		}

		ImGui.Spacing();
		//ImGui.Spacing();

		if (hasChildren) {
			if (!hideChildren || enabled) {
				ImGui.Indent();
				if (!compactMode)
					ImGui.Indent();

				foreach ((CustomComboPreset childPreset, CustomComboInfoAttribute childInfo) in children!) {
					this.drawPreset(childPreset, childInfo);
				}

				ImGui.Unindent();
				if (!compactMode)
					ImGui.Unindent();
			}
		}
	}

	private void enableParentPresets(CustomComboPreset original) {
		CustomComboPreset preset = original;

		while (this.childToParentPresets.TryGetValue(preset, out (CustomComboPreset Preset, CustomComboInfoAttribute Info) parent)) {

			if (!Service.Configuration.EnabledActions.Contains(parent.Preset)) {
				Service.Configuration.EnabledActions.Add(parent.Preset);

				foreach (CustomComboPreset conflict in parent.Preset.GetConflicts()) {
					Service.Configuration.EnabledActions.Remove(conflict);
				}
			}

			preset = parent.Preset;
		}
	}

}
