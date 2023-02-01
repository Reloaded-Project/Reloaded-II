namespace Reloaded.Mod.Launcher.Pages.BaseSubpages.ApplicationSubPages;

/// <summary>
/// Interaction logic for RunningProcessPage.xaml
/// </summary>
public partial class ReloadedProcessPage : ApplicationSubPage, IDisposable
{
    public ReloadedAppViewModel ViewModel { get; set; }
    private bool _disposed;

    public ReloadedProcessPage(ApplicationViewModel model)
    {
        SwappedOut += Dispose;
        InitializeComponent();
        ViewModel = new ReloadedAppViewModel(model);
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        ViewModel.Dispose();
    }

    private void Suspend_Click(object sender, System.Windows.RoutedEventArgs e) => ViewModel.Suspend();
    private void Resume_Click(object sender, System.Windows.RoutedEventArgs e) => ViewModel.Resume();
    private void Unload_Click(object sender, System.Windows.RoutedEventArgs e) => ViewModel.Unload();
    private void LoadMod_Click(object sender, System.Windows.RoutedEventArgs e) => ViewModel.ShowLoadModDialog();
}