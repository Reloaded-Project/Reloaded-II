namespace Reloaded.Mod.Launcher.Lib.Models.ViewModel.Dialog;

/// <summary>
/// A viewmodel for the Mod Update Dialog.
/// </summary>
public class ModUpdateDialogViewModel : ObservableObject
{
    /// <summary>
    /// The updater instance responsible for performing updates.
    /// </summary>
    public Updater Updater { get; set; }

    /// <summary>
    /// Provides easy access to the details of mod update to be performed.
    /// </summary>
    public ModUpdateSummary Summary { get; set; }

    /// <summary>
    /// Contains information about all mod updates to be performed.
    /// </summary>
    public ModUpdate[] UpdateInfo { get; set; }

    /// <summary>
    /// Total size of all mods to be downloaded.
    /// </summary>
    public long TotalSize { get; set; }

    /// <summary>
    /// Progress of the current update operation.
    /// Range 0 - 100.
    /// </summary>
    public double Progress { get; set; }

    /// <summary/>
    public ModUpdateDialogViewModel(Updater updater, ModUpdateSummary summary)
    {
        Updater = updater;
        Summary = summary;
        UpdateInfo = Summary.GetUpdateInfo();
        TotalSize = UpdateInfo.Sum(x => x.UpdateSize);
    }

    /// <summary>
    /// Performs an update of all mods.
    /// </summary>
    public async Task<bool> Update()
    {
        return await await ActionWrappers.ExecuteWithApplicationDispatcherAsync(PerformUpdateAsync);
    }

    private async Task<bool> PerformUpdateAsync()
    {
        if (ApplicationInstanceTracker.GetAllProcesses(out _))
        {
            Actions.DisplayMessagebox.Invoke(Resources.ErrorUpdateModInUseTitle.Get(), Resources.ErrorUpdateModInUse.Get(), new Actions.DisplayMessageBoxParams() { StartupLocation = Actions.WindowStartupLocation.CenterScreen, Type = Actions.MessageBoxType.Ok });
            return false;
        }

        await Updater.Update(Summary, new Progress<double>(d =>
        {
            Progress = d * 100;
        }));

        return true;
    }
}