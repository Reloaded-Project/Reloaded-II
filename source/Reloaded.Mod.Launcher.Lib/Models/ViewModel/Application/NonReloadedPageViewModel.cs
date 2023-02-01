namespace Reloaded.Mod.Launcher.Lib.Models.ViewModel.Application;

/// <summary>
/// ViewModel for a blank page that can be used to inject Reloaded into a process.
/// </summary>
public class NonReloadedPageViewModel : ObservableObject, IDisposable
{
    /// <summary>
    /// Parent ViewModel of this viewmodel.
    /// </summary>
    public ApplicationViewModel ApplicationViewModel { get; set; }

    /// <inheritdoc />
    public NonReloadedPageViewModel(ApplicationViewModel appViewModel)
    {
        ApplicationViewModel = appViewModel;
        ApplicationViewModel.SelectedProcess!.EnableRaisingEvents = true;
        ApplicationViewModel.SelectedProcess.Exited += SelectedProcessOnExited;
    }

    /// <summary/>
    ~NonReloadedPageViewModel() => Dispose();

    /// <inheritdoc />
    public void Dispose()
    {
        if (ApplicationViewModel.SelectedProcess != null)
            ApplicationViewModel.SelectedProcess!.Exited -= SelectedProcessOnExited;
        
        GC.SuppressFinalize(this);
    }

    private void SelectedProcessOnExited(object? sender, EventArgs e)
    {
        ActionWrappers.ExecuteWithApplicationDispatcherAsync(() =>
        {
            ApplicationViewModel.ChangeApplicationPage(ApplicationSubPage.ApplicationSummary);
        });
    }
}