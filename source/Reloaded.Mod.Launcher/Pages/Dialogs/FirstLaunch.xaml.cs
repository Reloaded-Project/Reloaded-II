namespace Reloaded.Mod.Launcher.Pages.Dialogs;

/// <summary>
/// Interaction logic for FirstLaunch.xaml
/// </summary>
public partial class FirstLaunch : ReloadedWindow
{
    public new FirstLaunchViewModel ViewModel { get; set; }
    
    public FirstLaunch()
    {
        InitializeComponent();
            
        ViewModel = Lib.IoC.Get<FirstLaunchViewModel>();
        
        // Disable Original Button Visibility
        base.ViewModel.CloseButtonVisibility = Visibility.Collapsed;
        base.ViewModel.MaximizeButtonVisibility = Visibility.Collapsed;
        base.ViewModel.MinimizeButtonVisibility = Visibility.Collapsed;

        // Init Events
        ViewModel.Initialize(() => ActionWrappers.ExecuteWithApplicationDispatcher(this.Close));
        this.Closing += (sender, args) => ViewModel.Dispose();
    }
}