namespace Reloaded.Mod.Launcher.Pages.BaseSubpages.ApplicationSubPages;

/// <summary>
/// The main page of the application.
/// </summary>
public partial class EditAppPage : ReloadedIIPage, IDisposable
{
    public EditAppViewModel ViewModel { get; set; }

    public EditAppPage() : base()
    {  
        InitializeComponent();

        // Setup ViewModel
        ViewModel = Lib.IoC.Get<EditAppViewModel>();
        this.DataContext = ViewModel;
        this.AnimateOutStarted += SaveSelectedItemOnAnimateOut;
        this.AnimateOutStarted += Dispose;
        Lib.IoC.Get<MainWindow>().Closing += OnMainWindowClosing;
    }

    public void Dispose()
    {
        Lib.IoC.Get<MainWindow>().Closing -= OnMainWindowClosing;
    }

    private async void SaveSelectedItemOnAnimateOut() => await ViewModel.SaveSelectedItemAsync();

    private async void OnMainWindowClosing(object sender, CancelEventArgs e) => await ViewModel.SaveSelectedItemAsync();

    private void Image_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e) => ViewModel.SetAppImage();

    private void UpdateExecutablePath_Click(object sender, System.Windows.RoutedEventArgs e) => ViewModel.SetNewExecutablePath();

    private void TestRepoConfiguration_Click(object sender, System.Windows.RoutedEventArgs e) => ViewModel.TestRepoConfiguration();
}