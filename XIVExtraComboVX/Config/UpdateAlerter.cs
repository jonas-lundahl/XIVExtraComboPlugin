namespace XIVExtraComboVX.Config;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Logging;

internal class UpdateAlerter: IDisposable {
	private const int messageDelayMs = 500;
	private const int loginDelayMs = 500;

	private bool disposed;

	private bool seenUpdateMessage = false;
	private readonly bool isFreshInstall = false;
	private readonly Version current;

	private CancellationTokenSource? realAborter;
	private CancellationTokenSource? aborter {
		get {
			CancellationTokenSource? src;
			lock (this) {
				src = this.realAborter;
			}
			return src;
		}
		set {
			lock (this) {
				this.realAborter?.Cancel();
				this.realAborter = value;
			}
		}
	}

	internal UpdateAlerter(Version to, bool isFresh) {
		this.current = to;
		this.isFreshInstall = isFresh;
		if (Service.Configuration.ShowUpdateMessage) {
			this.register();
			this.checkMessage();
		}
	}

	internal void checkMessage() {
		PluginLog.Information("Checking whether to display update message");
		if (this.disposed) {
			this.unregister();
			PluginLog.Information("Update alerter already disposed");
			return;
		}
		if (this.seenUpdateMessage) {
			this.unregister();
			PluginLog.Information("Message already displayed, unregistering");
			return;
		}
		try {
			if (!Service.GameState.isChatVisible) {
				PluginLog.Information("Chat is not visible, cannot display message yet");
				return;
			}
		}
		catch (Exception e) {
			PluginLog.Error(e, "Failed to check chat visibility state");
			this.unregister();
			return;
		}

		PluginLog.Information($"Checks passed, delaying message by {messageDelayMs}ms - may be reset if message is triggered again within that time");

		this.aborter = new();

		Task.Delay(messageDelayMs, this.aborter.Token).ContinueWith(waiter => {
			if (waiter.Status is TaskStatus.RanToCompletion)
				this.displayMessage();
		});
	}

	internal void displayMessage() {

		this.aborter?.Cancel();
		this.seenUpdateMessage = true;
		this.unregister();

		PluginLog.Information("Displaying update alert in game chat");

		List<Payload> parts = new();
		if (!(Plugin.Debug && Service.Interface.IsDev)) {
			parts.Add(new TextPayload(
				this.isFreshInstall
					? $"{Service.Plugin.ShortPluginSignature} has been installed. By default, all features are disabled.\n"
					: $"{Service.Plugin.Name} has been updated to {this.current}. Features may have been added or changed.\n"
			));
		}
		parts.AddRange(new Payload[] {
			new UIForegroundPayload(ChatUtil.clfgOpenConfig),
			new UIGlowPayload(ChatUtil.clbgOpenConfig),
			Service.ChatUtils.clplOpenConfig,
			new TextPayload($"[Open {Service.Plugin.Name} Settings]"),
			RawPayload.LinkTerminator,
			new UIGlowPayload(0),
			new UIForegroundPayload(0),
		});

		Service.ChatUtils.print(XivChatType.Notice, parts.ToArray());

	}

	internal void register() {
		PluginLog.Information("Registering update alerter");
		Service.ChatGui.ChatMessage += this.onChatMessage;
		Service.Client.Login += this.onLogin;
	}

	internal void unregister() {
		PluginLog.Information("Unregistering update alerter");
		Service.ChatGui.ChatMessage -= this.onChatMessage;
		Service.Client.Login -= this.onLogin;
	}

	private void onChatMessage(XivChatType type, uint senderId, ref SeString sender, ref SeString message, ref bool isHandled) {
		if (type is XivChatType.Urgent or XivChatType.Notice or XivChatType.SystemMessage)
			this.checkMessage();
	}
	private async void onLogin(object? sender, EventArgs e) {
		await Task.Delay(messageDelayMs + 1000);
		while (!Service.Client.IsLoggedIn)
			await Task.Delay(loginDelayMs);
		this.checkMessage();
	}


	#region Disposing

	public void Dispose() {
		if (this.disposed)
			return;
		this.disposed = true;

		this.unregister();
	}

	#endregion

}
