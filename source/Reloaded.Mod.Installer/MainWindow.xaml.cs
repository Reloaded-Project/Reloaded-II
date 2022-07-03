namespace Reloaded.Mod.Installer;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : GlowWindow
{
    public MainWindowViewModel ViewModel { get; set; }

    public Task InstallTask { get; set; }

    public MainWindow()
    {
        ViewModel = new MainWindowViewModel();
        InitializeComponent();
    }
    
    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        InstallTask = ViewModel.InstallReloadedAsync();
        await InstallTask.ConfigureAwait(false);
        Application.Current.Shutdown(0);
    }

    private async void OnClosing(object sender, CancelEventArgs e)
    {
        ViewModel.CancellationToken.Cancel();
        await InstallTask;
    }
}