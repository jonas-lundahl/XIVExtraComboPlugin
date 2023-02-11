namespace XIVExtraComboVX.GameData;

using System;

using FFXIVClientStructs.FFXIV.Component.GUI;

internal unsafe class GameState: IDisposable {
	private bool disposed;

	private AtkUnitBase* chatLogPointer;

	internal AtkUnitBase* chatLog {
		get {
			if (Service.Client.LocalPlayer is null)
				return null;
			if (this.chatLogPointer is null)
				this.chatLogPointer = (AtkUnitBase*)Service.GameGui.GetAddonByName("ChatLog", 1);
			return this.chatLogPointer;
		}
	}

	internal bool isChatVisible {
		get {
			AtkUnitBase* cl = this.chatLog;
			return cl is not null && cl->IsVisible;
		}
	}

	#region Registration and cleanup

	internal GameState() {
		Service.Client.Logout += this.clearCacheOnLogout;
	}

	private void clearCacheOnLogout(object? sender, EventArgs args) => this.chatLogPointer = null;

	public void Dispose() {
		if (this.disposed)
			return;
		this.disposed = true;

		Service.Client.Logout -= this.clearCacheOnLogout;
		this.chatLogPointer = null;
	}

	#endregion

}
