using Page = Reloaded.Mod.Launcher.Lib.Models.Model.Pages.Page;
using Window = System.Windows.Window;

namespace Reloaded.Mod.Launcher.Pages;

/// <summary>
/// The main page of the application.
/// </summary>
public partial class BasePage : ReloadedIIPage, IDisposable
{
    public MainPageViewModel ViewModel { get; set; }

    private CollectionViewSource _appsViewSource;
    private Window? _owner;

    public BasePage() : base()
    {
        InitializeComponent();
        ViewModel = Lib.IoC.Get<MainPageViewModel>();
        var manipulator = new DictionaryResourceManipulator(this.Contents.Resources);
        _appsViewSource = manipulator.Get<CollectionViewSource>("FilteredApps");
        this.Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _owner = Window.GetWindow(this);
        _owner!.KeyDown += TrySwitchPage;
        ControllerSupport.SubscribeCustomInputs(ProcessCustomInputs);
        this.Loaded -= OnLoaded;
    }

    ~BasePage() => Dispose();

    public void Dispose()
    {
        _owner!.KeyDown -= TrySwitchPage;
        ControllerSupport.UnsubscribeCustomInputs(ProcessCustomInputs);
        GC.SuppressFinalize(this);
    }

    /* Preconfigured Buttons */
    private void AddApp_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.AddApplicationCommand.Execute(null);
    }

    private void ManageMods_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.Page = Page.ManageMods;
    }

    private void LoaderSettings_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.Page = Page.SettingsPage;
    }

    private void DownloadMods_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.Page = Page.DownloadMods;
    }

    private void Application_Click(object sender, RoutedEventArgs e)
    {
        // Prepare for parameter transfer.
        if (sender is not FrameworkElement element) 
            return;

        if (element.DataContext is not PathTuple<ApplicationConfig> tuple) 
            return;

        ViewModel.SwitchToApplication(tuple);
    }

    private void ProcessCustomInputs(in ControllerState state, ref bool handled)
    {
        if (!Window.GetWindow(this)!.IsActive)
            return;

        if (!ControllerSupport.TryGetPageScrollDirection(state, out var direction))
            return;

        ViewModel.SwitchPage(direction, _appsViewSource.View.Cast<PathTuple<ApplicationConfig>>().ToArray());
    }

    private void TrySwitchPage(object sender, KeyEventArgs e)
    {
        if (!KeyboardUtils.TryGetPageScrollDirection(e, out int direction))
            return;
        
        ViewModel.SwitchPage(direction, _appsViewSource.View.Cast<PathTuple<ApplicationConfig>>().ToArray());
    }
}