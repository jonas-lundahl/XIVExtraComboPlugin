namespace XIVExtraComboVX;

using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

using Dalamud.Plugin.Ipc;
using Dalamud.Plugin.Ipc.Exceptions;

internal class Ipc: IDisposable {
	private const int loopDelaySec = 300;
	private const int initialDelaySec = 5;

	private const string tippyPluginId = "Tippy";
	private const string tippyRegisterTipId = "Tippy.RegisterTip";
	private const string tippyRegisterMessageId = "Tippy.RegisterMessage";

	private readonly CancellationTokenSource stop = new();
	private readonly Task registrationWorker;
	private bool disposed;

	private bool tippyFound;
	internal bool tippyLoaded {
		get {
			if (!this.tippyFound)
				this.tippyFound = Service.Interface.PluginInternalNames.Contains(tippyPluginId);
			return this.tippyFound;
		}
	}

	private readonly ICallGateSubscriber<string, bool> tippyRegisterTip;
	private readonly ICallGateSubscriber<string, bool> tippyRegisterMessage;

	private readonly ConcurrentQueue<string> tippyRegistrationQueue = new();

	internal Ipc() {
		this.tippyRegisterTip = Service.Interface.GetIpcSubscriber<string, bool>(tippyRegisterTipId);
		this.tippyRegisterMessage = Service.Interface.GetIpcSubscriber<string, bool>(tippyRegisterMessageId);
		this.registrationWorker = Task.Run(this.registrationLoop, this.stop.Token);
	}

	private async void registrationLoop() {
		await Task.Delay(initialDelaySec * 1000, this.stop.Token);
		while (!this.stop.IsCancellationRequested) {
			try { // catches cancellation so the task shows as completed, once it's actually gotten started

				if (this.tippyLoaded) {
					if (!this.tippyRegistrationQueue.IsEmpty) {
						string tippyTip = null!;
						try {
							while (this.tippyRegistrationQueue.TryDequeue(out tippyTip!)) {
								if (!string.IsNullOrWhiteSpace(tippyTip))
									this.tippyRegisterTip.InvokeFunc(tippyTip); // ignore the return value cause if Tippy just says "no" then what are WE gonna do about it?
							}
						}
						catch (IpcNotReadyError) {
							// We already got the value out of the queue, but registering it failed, so put it back in
							this.addTips(tippyTip);
						}
						catch (IpcError ex) {
							Service.Logger.error("Failed to register tip for Tippy's pool", ex);
							this.tippyRegistrationQueue.Clear();
						}
					}
				}

				// Only try to do things every so often
				await Task.Delay(loopDelaySec * 1000, this.stop.Token);
			}
			catch (TaskCanceledException) { // if cancellation is requested once we get started, it's considered completion cause this is infinite
				return;
			}
		}
	}

	#region Tippy API
	internal void addTips(params string[] tips) {
		if (this.disposed)
			return;

		foreach (string tip in tips) {
			if (!string.IsNullOrWhiteSpace(tip))
				this.tippyRegistrationQueue.Enqueue(tip);
		}
	}

	internal bool showTippyMessage(string message) {
		if (this.disposed)
			return false;

		try {
			return this.tippyRegisterMessage.InvokeFunc(message);
		}
		catch (IpcNotReadyError) {
			return false;
		}
		catch (IpcError ex) {
			Service.Logger.error("Failed to register priority message for Tippy", ex);
			return false;
		}
	}
	#endregion

	#region Disposable
	protected virtual void Dispose(bool disposing) {
		if (this.disposed)
			return;
		this.disposed = true;

		if (disposing) {
			this.stop.Cancel();
			this.registrationWorker.Wait();
			this.registrationWorker.Dispose();
			this.stop.Dispose();
			this.tippyRegistrationQueue.Clear();
		}

	}

	public void Dispose() {
		this.Dispose(true);
		GC.SuppressFinalize(this);
	}
	#endregion

}
