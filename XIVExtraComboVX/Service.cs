namespace XIVExtraComboVX;

using Dalamud.Data;
using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Buddy;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.JobGauge;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.Command;
using Dalamud.Game.Gui;
using Dalamud.IoC;
using Dalamud.Plugin;

using XIVExtraComboVX.Config;
using XIVExtraComboVX.GameData;

internal class Service {
	public static Plugin Plugin { get; set; } = null!;

	public static PluginConfiguration Configuration { get; set; } = null!;

	public static IconReplacer IconReplacer { get; set; } = null!;

	public static PluginAddressResolver Address { get; set; } = null!;

	public static ComboDataCache DataCache { get; set; } = null!;

	public static LogUtil Logger { get; set; } = null!;

	public static GameState GameState { get; set; } = null!;

	public static UpdateAlerter? UpdateAlert { get; set; } = null;

	public static ChatUtil ChatUtils { get; set; } = null!;

	public static Ipc Ipc { get; set; } = null!;

	[PluginService]
	public static DalamudPluginInterface Interface { get; private set; } = null!;

	[PluginService]
	public static BuddyList BuddyList { get; private set; } = null!;

	[PluginService]
	public static ChatGui ChatGui { get; private set; } = null!;

	[PluginService]
	public static ClientState Client { get; private set; } = null!;

	[PluginService]
	public static CommandManager Commands { get; private set; } = null!;

	[PluginService]
	public static Condition Conditions { get; private set; } = null!;

	[PluginService]
	public static DataManager GameData { get; private set; } = null!;

	[PluginService]
	public static Framework Framework { get; private set; } = null!;

	[PluginService]
	public static JobGauges JobGauge { get; private set; } = null!;

	[PluginService]
	public static TargetManager Targets { get; private set; } = null!;

	[PluginService]
	public static GameGui GameGui { get; private set; } = null!;

}
