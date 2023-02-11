namespace XIVExtraComboVX;

using System;
using System.Collections.Generic;
using System.Reflection;

using Dalamud.Game.Command;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Interface.Windowing;
using Dalamud.Logging;
using Dalamud.Plugin;

using XIVExtraComboVX.Config;

public sealed class Plugin: IDalamudPlugin {
	private bool disposed = false;

	internal const string command = "/pcombo";

	private readonly WindowSystem? windowSystem;
	private readonly ConfigWindow? configWindow;
	private readonly bool registeredDefaultCommand = false;

	public static readonly Version Version = Assembly.GetExecutingAssembly().GetName().Version!;
	public const bool Debug =
#if DEBUG
		true;
#else
		false;
#endif

#pragma warning disable CA1822 // Mark members as static (some of these shouldn't be used until the plugin is initialised)
	public string Name { get; } = Assembly.GetExecutingAssembly().GetName().Name!;
	public string PluginBuildType { get; } = $"{(Debug ? "debug" : "release")} build";
	public string PluginInstallType => $"{(Service.Interface.IsDev ? "dev" : "standard")} install";
	public string ShortPluginSignature => $"{this.Name} v{Version}";
	public string FullPluginSignature => $"{this.ShortPluginSignature} ({this.PluginBuildType}, {this.PluginInstallType})";
#pragma warning restore CA1822 // Mark members as static

	public Plugin(DalamudPluginInterface pluginInterface) {

		pluginInterface.Create<Service>();

		Service.Plugin = this;
		Service.Logger = new();
		Service.Configuration = pluginInterface.GetPluginConfig() as PluginConfiguration ?? new(true);
		Service.Address = new();

		Service.Configuration.Active = true;
		Service.Configuration.UpgradeIfNeeded();

		Service.Address.Setup();

		if (Service.Address.LoadSuccessful) {
			Service.DataCache = new();
			Service.IconReplacer = new();
			Service.GameState = new();
			Service.ChatUtils = new();

			this.configWindow = new();
			this.windowSystem = new(this.GetType().Namespace!);
			this.windowSystem.AddWindow(this.configWindow);

			Service.Interface.UiBuilder.OpenConfigUi += this.toggleConfigUi;
			Service.Interface.UiBuilder.Draw += this.windowSystem.Draw;
		}
		else {
			Service.Commands.ProcessCommand("/xllog");
		}

		CommandInfo handler = new(this.onPluginCommand) {
			HelpMessage = Service.Address.LoadSuccessful ? "Open a window to edit custom combo settings." : "Do nothing, because the plugin failed to initialise.",
			ShowInHelp = true
		};

		Service.Commands.AddHandler(command + "vx", handler);
		if (Service.Configuration.RegisterCommonCommand) {
			Service.Commands.AddHandler(command, handler);
			this.registeredDefaultCommand = true;
		}

		Service.Ipc = new();

		Service.Ipc.addTips(
			$"{this.Name} - better than a broken leg!" // I will not be serious and you cannot make me.
		);

		PluginLog.Information($"{this.FullPluginSignature} initialised {(Service.Address.LoadSuccessful ? "" : "un")}successfully");
		if (Service.Configuration.IsFirstRun || !Service.Configuration.LastVersion.Equals(Version)) {
			Service.UpdateAlert = new(Version, Service.Configuration.IsFirstRun);

			Service.Configuration.LastVersion = Version;
			Service.Configuration.Save();
		}

	}

	#region Disposable

	public void Dispose() {
		this.dispose(true);
		GC.SuppressFinalize(this);
	}

	private void dispose(bool disposing) {
		if (this.disposed)
			return;
		this.disposed = true;

		if (disposing) {
			Service.Commands.RemoveHandler(command + "vx");
			if (this.registeredDefaultCommand)
				Service.Commands.RemoveHandler(command);

			Service.Interface.UiBuilder.OpenConfigUi -= this.toggleConfigUi;
			if (this.windowSystem is not null)
				Service.Interface.UiBuilder.Draw -= this.windowSystem.Draw;

			Service.IconReplacer?.Dispose();
			Service.DataCache?.Dispose();
			Service.UpdateAlert?.Dispose();
			Service.ChatUtils?.Dispose();
			Service.GameState?.Dispose();
			Service.Ipc?.Dispose();
			Service.Logger?.Dispose();
		}
	}

	#endregion

	internal void toggleConfigUi() {
		if (this.configWindow is not null) {
			this.configWindow.IsOpen = !this.configWindow.IsOpen;
		}
		else {
			PluginLog.Error("Cannot toggle configuration window, reference does not exist");
		}
	}

	internal void onPluginCommand(string command, string arguments) {
		if (!Service.Address.LoadSuccessful) {
			Service.ChatGui.PrintError($"The plugin failed to initialise and cannot run:\n{Service.Address.LoadFailReason!.Message}");
			return;
		}

		string[] argumentsParts = arguments.Split();

		switch (argumentsParts[0].ToLower()) {
			case "enable": {
					Service.Configuration.Active = true;
					Service.ChatUtils.print(XivChatType.Notice,
						new UIForegroundPayload(35),
						new TextPayload(Service.Plugin.Name),
						new UIForegroundPayload(1),
						new TextPayload(" "),
						new UIGlowPayload(43),
						new TextPayload("enabled"),
						new UIGlowPayload(0),
						new UIForegroundPayload(0)
					);
				}
				break;
			case "disable": {
					Service.Configuration.Active = false;
					Service.ChatUtils.print(XivChatType.Notice,
						new UIForegroundPayload(35),
						new TextPayload(Service.Plugin.Name),
						new UIForegroundPayload(1),
						new TextPayload(" "),
						new UIGlowPayload(17),
						new TextPayload("disabled"),
						new UIGlowPayload(0),
						new UIForegroundPayload(0)
					);
				}
				break;
			case "toggle": {
					bool on = !Service.Configuration.Active;
					Service.Configuration.Active = on;
					Service.ChatUtils.print(XivChatType.Notice,
						new UIForegroundPayload(35),
						new TextPayload(Service.Plugin.Name),
						new UIForegroundPayload(1),
						new TextPayload(" "),
						new UIGlowPayload((ushort)(on ? 43 : 17)),
						new TextPayload($"{(on ? "en" : "dis")}abled"),
						new UIGlowPayload(0),
						new UIForegroundPayload(0)
					);
				}
				break;
			case "debug": {
					Service.Logger.EnableNextTick();
					Service.ChatGui.Print("Enabled debug message snapshot");
				}
				break;
			case "version": {
					Service.ChatGui.Print($"You are running {this.FullPluginSignature}");
				}
				break;
			case "reset": {
					PluginConfiguration config = new(false) {
						LastVersion = Plugin.Version
					};
					Service.Configuration = config;
					config.Save();
					List<Payload> parts = new(new Payload[] {
						new TextPayload("Your "),
						new UIForegroundPayload(35),
						new TextPayload(Service.Plugin.Name),
						new UIForegroundPayload(0),
						new TextPayload("configuration has been reset to the defaults.")
					});
					if (this.configWindow is not null && !this.configWindow.IsOpen) {
						parts.AddRange(new Payload[] {
							new TextPayload("\nYou will need to "),
							new UIForegroundPayload(ChatUtil.clfgOpenConfig),
							new UIGlowPayload(ChatUtil.clbgOpenConfig),
							Service.ChatUtils.clplOpenConfig,
							new TextPayload($"[open the settings]"),
							RawPayload.LinkTerminator,
							new UIGlowPayload(0),
							new UIForegroundPayload(0),
							new TextPayload(" to enable your desired features.")
						});
					}
					Service.ChatUtils.print(XivChatType.Notice, parts.ToArray());
				}
				break;
			case "showupdate": {
					Service.UpdateAlert?.displayMessage();
				}
				break;
			default:
				this.toggleConfigUi();
				break;
		}

		Service.Interface.SavePluginConfig(Service.Configuration);
	}

}
