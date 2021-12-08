using Reloaded.Mod.Launcher.Lib.Models.ViewModel;
using Reloaded.Mod.Launcher.Models.ViewModel.ApplicationSubPages;

namespace Reloaded.Mod.Launcher.Pages.BaseSubpages.ApplicationSubPages;

/// <summary>
/// Interaction logic for RunningProcessPage.xaml
/// </summary>
public partial class ReloadedProcessPage : ApplicationSubPage
{
    public ReloadedAppViewModel ViewModel { get; set; }

    public ReloadedProcessPage(ApplicationViewModel model)
    {
        InitializeComponent();
        ViewModel = new ReloadedAppViewModel(model);
        this.AnimateOutStarted += Dispose;
    }

    private void Dispose()
    {
        this.AnimateOutStarted -= Dispose;
        ViewModel.Dispose();
    }

    private void Suspend_Click(object sender, System.Windows.RoutedEventArgs e) => ViewModel.Suspend();
    private void Resume_Click(object sender, System.Windows.RoutedEventArgs e) => ViewModel.Resume();
    private void Unload_Click(object sender, System.Windows.RoutedEventArgs e) => ViewModel.Unload();
    private void LoadMod_Click(object sender, System.Windows.RoutedEventArgs e) => ViewModel.ShowLoadModDialog();
}