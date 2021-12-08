using System;
using Reloaded.Mod.Launcher.Lib.Models.Model.Pages;
using Reloaded.Mod.Launcher.Lib.Models.ViewModel;
using Reloaded.Mod.Loader.IO.Utility;

namespace Reloaded.Mod.Launcher.Models.ViewModel.ApplicationSubPages;

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
        ApplicationViewModel.SelectedProcess!.Exited -= SelectedProcessOnExited;
        GC.SuppressFinalize(this);
    }

    private void SelectedProcessOnExited(object? sender, EventArgs e) => ApplicationViewModel.ChangeApplicationPage(ApplicationSubPage.ApplicationSummary);
}