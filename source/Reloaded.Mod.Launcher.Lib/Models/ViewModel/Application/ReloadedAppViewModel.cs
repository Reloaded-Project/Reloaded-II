using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Reloaded.Mod.Launcher.Lib;
using Reloaded.Mod.Launcher.Lib.Models.Model.Pages;
using Reloaded.Mod.Launcher.Lib.Models.ViewModel;
using Reloaded.Mod.Launcher.Lib.Models.ViewModel.Dialog;
using Reloaded.Mod.Launcher.Lib.Static;
using Reloaded.Mod.Launcher.Lib.Utility;
using Reloaded.Mod.Loader.IO.Utility;
using Reloaded.Mod.Loader.Server;
using Reloaded.Mod.Loader.Server.Messages.Response;
using Reloaded.Mod.Loader.Server.Messages.Structures;

namespace Reloaded.Mod.Launcher.Models.ViewModel.ApplicationSubPages;

/// <summary>
/// ViewModel for an individual application with Reloaded loaded inside it.
/// </summary>
public class ReloadedAppViewModel : ObservableObject, IDisposable
{
    private static int ClientLoaderSetupTimeout { get; set; } = 1000;
    private static int ClientLoaderSetupSleepTime { get; set; } = 32;
    private static int ClientModListRefreshInterval { get; set; } = 100;

    /// <summary>
    /// ViewModel of the application being manipulated.
    /// </summary>
    public ApplicationViewModel          ApplicationViewModel   { get; set; }
    
    /// <summary>
    /// Client used to connect to the server on the remote process.
    /// </summary>
    public Client?                        Client                { get; set; }

    /// <summary>
    /// The currently highlighted mod.
    /// </summary>
    public ModInfo                       SelectedMod            { get; set; } = null!;

    /// <summary>
    /// List of currently loaded mods.
    /// </summary>
    public ObservableCollection<ModInfo> CurrentMods            { get; set; } = new ObservableCollection<ModInfo>();

    private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
    private System.Timers.Timer? _refreshTimer;

    /// <summary/>
    public ReloadedAppViewModel(ApplicationViewModel applicationViewModel)
    {
        ApplicationViewModel = applicationViewModel;
        ApplicationViewModel.SelectedProcess!.EnableRaisingEvents = true;
        ApplicationViewModel.SelectedProcess.Exited += SelectedProcessOnExited;

        /* Try establish connection. */
        int port = 0;
        try
        {
            port = ActionWrappers.TryGetValue(GetPort, ClientLoaderSetupTimeout, ClientLoaderSetupSleepTime);
        }
        catch (Exception ex)
        {
            Errors.HandleException(new Exception(Resources.ErrorFailedToObtainPort.Get(), ex));
            return;
        }

        Client = new Client(port);
        Client.OnReceiveException += ClientOnReceiveException;
        Refresh();

        _refreshTimer = new System.Timers.Timer(ClientModListRefreshInterval);
        _refreshTimer.AutoReset = true;
        _refreshTimer.Elapsed += (sender, args) => Refresh();
        _refreshTimer.Enabled = true;
    }

    /// <summary/>
    ~ReloadedAppViewModel() => Dispose();

    /// <inheritdoc />
    public void Dispose()
    {
        if (Client != null)
            Client.OnReceiveException -= ClientOnReceiveException;
        
        _cancellationTokenSource?.Cancel();
        _refreshTimer?.Dispose();
    }

    private void SelectedProcessOnExited(object? sender, EventArgs e)
    {
        ApplicationViewModel.SelectedProcess!.Exited -= SelectedProcessOnExited;
        ApplicationViewModel.ChangeApplicationPage(ApplicationSubPage.ApplicationSummary);
    }

    private void ClientOnReceiveException(GenericExceptionResponse obj) => Errors.HandleException(new Exception(obj.Message));

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

    Task<Acknowledgement>?       UnloadTask()   => Client?.UnloadModAsync(SelectedMod.ModId, 1000, _cancellationTokenSource.Token);
    Task<Acknowledgement>?       SuspendTask()  => Client?.SuspendModAsync(SelectedMod.ModId, 1000, _cancellationTokenSource.Token);
    Task<Acknowledgement>?       ResumeTask()   => Client?.ResumeModAsync(SelectedMod.ModId, 1000, _cancellationTokenSource.Token);
    Task<GetLoadedModsResponse>? RefreshTask()  => Client?.GetLoadedModsAsync(1000, _cancellationTokenSource.Token);

    /// <summary>
    /// Refreshes the list of loaded mods.
    /// </summary>
    public async void Refresh()
    {
        try
        {
            var loadedMods = await Task.Run(RefreshTask).ConfigureAwait(false);
            ActionWrappers.ExecuteWithApplicationDispatcher(() =>
            {
                Collections.ModifyObservableCollection(CurrentMods, loadedMods.Mods);
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
        return Client.GetPort(pid);
    }

    /// <summary>
    /// Shows a dialog that can be used to select a mod to be loaded.
    /// </summary>
    public void ShowLoadModDialog()
    {
        Actions.ShowModLoadSelectDialog(new LoadModSelectDialogViewModel(ApplicationViewModel, this));
    }
}