namespace Reloaded.Mod.Launcher.Lib.Models.ViewModel.Application;

/// <summary>
/// ViewModel for an individual application with Reloaded loaded inside it.
/// </summary>
public class ReloadedAppViewModel : ObservableObject, IDisposable
{
    private const int RequestTimeout = 8000;
    private static int ClientLoaderSetupTimeout { get; set; } = 1000;
    private static int ClientLoaderSetupSleepTime { get; set; } = 32;
    private static int ClientModListRefreshInterval { get; set; } = 125;

    /// <summary>
    /// ViewModel of the application being manipulated.
    /// </summary>
    public ApplicationViewModel ApplicationViewModel { get; set; }
    
    /// <summary>
    /// Client used to connect to the server on the remote process.
    /// </summary>
    public LiteNetLibClient? Client { get; set; }

    /// <summary>
    /// The currently highlighted mod.
    /// </summary>
    public ServerModInfo SelectedMod { get; set; } = null!;

    /// <summary>
    /// List of currently loaded mods.
    /// </summary>
    public ObservableCollection<ServerModInfo> CurrentMods { get; set; } = new ObservableCollection<ServerModInfo>();

    private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
    private System.Timers.Timer? _refreshTimer;
    private int _port;

    /// <summary/>
    public ReloadedAppViewModel(ApplicationViewModel applicationViewModel)
    {
        ApplicationViewModel = applicationViewModel;
        ApplicationViewModel.SelectedProcess!.EnableRaisingEvents = true;
        ApplicationViewModel.SelectedProcess.Exited += SelectedProcessOnExited;

        /* Try establish connection. */
        if (!TryGetPort(true, out _port)) 
            return;

        Client = new LiteNetLibClient(IPAddress.Loopback, "", _port, true);
        Client.OnTryReconnect += (peer) => TryGetPort(false, out _port);
        Client.OverrideDetailsOnReconnect += () => (null, _port);
        Client.OnReceiveException += ClientOnReceiveException;

        _refreshTimer = new System.Timers.Timer(ClientModListRefreshInterval);
        _refreshTimer.AutoReset = true;
        _refreshTimer.Elapsed += (sender, args) =>
        {
            if (Client is { IsConnected: true })
                Refresh();
        };
        _refreshTimer.Enabled = true;
    }

    private bool TryGetPort(bool throwOnFailure, out int port)
    {
        try
        {
            port = ActionWrappers.TryGetValue(GetPort, ClientLoaderSetupTimeout, ClientLoaderSetupSleepTime);
            return true;
        }
        catch (Exception ex)
        {
            port = -1;
            if (throwOnFailure)
                Errors.HandleException(new Exception(Resources.ErrorFailedToObtainPort.Get(), ex));
            
            return false;
        }
    }

    /// <summary/>
    ~ReloadedAppViewModel() => Dispose();

    /// <inheritdoc />
    public void Dispose()
    {
        if (Client != null)
            Client.OnReceiveException -= ClientOnReceiveException;

        if (ApplicationViewModel.SelectedProcess != null)
            ApplicationViewModel.SelectedProcess.Exited -= SelectedProcessOnExited;
        
        _cancellationTokenSource?.Cancel();
        _refreshTimer?.Dispose();
    }

    private void SelectedProcessOnExited(object? sender, EventArgs e)
    {
        ApplicationViewModel.SelectedProcess!.Exited -= SelectedProcessOnExited;
        ActionWrappers.ExecuteWithApplicationDispatcherAsync(() =>
        {
            ApplicationViewModel.ChangeApplicationPage(ApplicationSubPage.ApplicationSummary);
        });
    }

    private void ClientOnReceiveException(AcknowledgementOrExceptionResponse obj) => Errors.HandleException(new Exception(obj.Message));

    /// <summary>
    /// Resets the selected index.
    /// </summary>
    public void ResetSelectedMod()
    {
        ActionWrappers.TryCatchDiscard(() => SelectedMod = CurrentMods[0]);
    }

    /* Actions */

    /// <summary>
    /// Can be used to unload the currently highlighted mod.
    /// </summary>
    public void Unload()    => Task.Run(UnloadTask).ContinueWith((_) => Refresh());
    
    /// <summary>
    /// Can be used to suspend the currently suspend mod.
    /// </summary>
    public void Suspend()   => Task.Run(SuspendTask).ContinueWith((_) => Refresh());

    /// <summary>
    /// Can be used to resume a suspended mod.
    /// </summary>
    public void Resume()    => Task.Run(ResumeTask).ContinueWith((_) => Refresh());

    Task<AnyOf<AcknowledgementOrExceptionResponse, NullResponse>>?       UnloadTask()   => Client?.UnloadModAsync(SelectedMod.ModId, RequestTimeout, _cancellationTokenSource.Token);
    Task<AnyOf<AcknowledgementOrExceptionResponse, NullResponse>>?       SuspendTask()  => Client?.SuspendModAsync(SelectedMod.ModId, RequestTimeout, _cancellationTokenSource.Token);
    Task<AnyOf<AcknowledgementOrExceptionResponse, NullResponse>>?       ResumeTask()   => Client?.ResumeModAsync(SelectedMod.ModId, RequestTimeout, _cancellationTokenSource.Token);
    Task<AnyOf<AcknowledgementOrExceptionResponse, GetLoadedModsResponse>>? RefreshTask()  => Client?.GetLoadedModsAsync(RequestTimeout, _cancellationTokenSource.Token);

    /// <summary>
    /// Refreshes the list of loaded mods.
    /// </summary>
    public async void Refresh()
    {
        try
        {
            var loadedMods = await Task.Run(RefreshTask).ConfigureAwait(false);
            if (!loadedMods.IsSecond)
                return;

            ActionWrappers.ExecuteWithApplicationDispatcher(() =>
            {
                Collections.ModifyObservableCollection(CurrentMods, loadedMods.Second.Mods);
                RaisePropertyChangedEvent(nameof(CurrentMods));
            });
        }
        catch (Exception)
        {
            // ignored
        }
    }

    /// <summary>
    /// Acquires the port to connect to the remote process.
    /// </summary>
    private int GetPort()
    {
        int pid = ApplicationViewModel.SelectedProcess!.Id;
        return ServerUtility.GetPort(pid);
    }

    /// <summary>
    /// Shows a dialog that can be used to select a mod to be loaded.
    /// </summary>
    public void ShowLoadModDialog()
    {
        Actions.ShowModLoadSelectDialog(new LoadModSelectDialogViewModel(ApplicationViewModel, this));
    }
}