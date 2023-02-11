namespace XIVExtraComboVX;

using System;

using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;

internal class ChatUtil: IDisposable {
	private bool disposed;

	private const uint
		clidOpenConfig = 0;

	internal readonly DalamudLinkPayload
		clplOpenConfig;

	internal const ushort
		clfgOpenConfig = 34,
		clbgOpenConfig = 502;

	internal ChatUtil() {
		this.clplOpenConfig = Service.Interface.AddChatLinkHandler(clidOpenConfig, this.onClickChatLink);
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Convention")]
	internal void print(XivChatType type, params Payload[] payloads) {
		if (payloads.Length > 0) {
			Service.ChatGui.PrintChat(new XivChatEntry() {
				Type = type,
				Message = new SeString(payloads),
			});
		}
	}

	private void onClickChatLink(uint id, SeString source) {
		switch (id) {
			case clidOpenConfig:
				Service.Plugin.onPluginCommand("", "");
				break;
			default:
				Service.ChatGui.PrintChat(new XivChatEntry() {
					Type = XivChatType.SystemError,
					Message = new SeString(
						new TextPayload($"An internal error has occured: no handler is registered for id {id}.")
					),
				});
				break;
		}
	}

	#region Disposable

	public void Dispose() {
		if (this.disposed)
			return;
		this.disposed = true;

		Service.Interface.RemoveChatLinkHandler();
	}

	#endregion

}
