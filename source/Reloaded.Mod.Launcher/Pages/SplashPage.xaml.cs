using MessageBox = Reloaded.Mod.Launcher.Pages.Dialogs.MessageBox;
using Paths = Reloaded.Mod.Loader.IO.Paths;
using Window = System.Windows.Window;
using WindowViewModel = Reloaded.Mod.Launcher.Lib.Models.ViewModel.WindowViewModel;

namespace Reloaded.Mod.Launcher.Pages;

/// <summary>
/// The main page of the application.
/// </summary>
public partial class SplashPage : ReloadedIIPage
{
    private readonly SplashViewModel _splashViewModel;
    private XamlResource<int> _xamlSplashMinimumTime = new XamlResource<int>("SplashMinimumTime");
    private XamlResource<string> _xamlFailedToLaunchTitle = new XamlResource<string>("FailedToLaunchTitle");
    private XamlResource<string> _xamlFailedToLaunchMessage = new XamlResource<string>("FailedToLaunchMessage");
    private Task _setupApplicationTask;
    private List<Task> _backgroundTasks = new List<Task>();

    public SplashPage() : base()
    {  
        InitializeComponent();

        // Setup ViewModel
        _splashViewModel    = new SplashViewModel();
        this.DataContext    = _splashViewModel;
        _setupApplicationTask = Task.Run(() => Setup.SetupApplicationAsync(UpdateText, _xamlSplashMinimumTime.Get(), _backgroundTasks));
        ActionWrappers.ExecuteWithApplicationDispatcherAsync(Load);
    }

    private async void Load()
    {
        try
        {
            await _setupApplicationTask;
            ChangeToMainPage();
            _backgroundTasks.Add(Task.Run(ControllerSupport.Init));

            // Post init cleanup.
            _ = Task.WhenAll(_backgroundTasks).ContinueWith(task =>
            {
                App.StopProfileOptimization();
                GC.Collect(int.MaxValue, GCCollectionMode.Optimized, true, true);
                App.EmptyWorkingSet();
            });

            DisplayFirstLaunchWarningIfNeeded();
        }
        catch (Exception ex)
        {
            var messageBox = new MessageBox(_xamlFailedToLaunchTitle.Get(), _xamlFailedToLaunchMessage.Get() + $" {ex.Message}\n{ex.StackTrace}");
            messageBox.ShowDialog();
        }
    }

    private void ChangeToMainPage()
    {
        var viewModel = Lib.IoC.Get<WindowViewModel>();
        viewModel.CurrentPage = PageBase.Base;
    }

    private void DisplayFirstLaunchWarningIfNeeded()
    {
        var loaderConfig = Lib.IoC.Get<LoaderConfig>();
        if (loaderConfig.FirstLaunch)
        {
            IConfig<LoaderConfig>.ToPath(loaderConfig, Paths.LoaderConfigPath);

            var firstLaunchWindow   = new FirstLaunch();
            firstLaunchWindow.Owner = Window.GetWindow(this);
            firstLaunchWindow.ShowDialog();
            loaderConfig.FirstLaunch = false;
        }
    }

    /// <summary>
    /// Runs a method intended to update the UI thread.
    /// </summary>
    private void UpdateText(string newText)
    {
        ActionWrappers.ExecuteWithApplicationDispatcherAsync(() => { _splashViewModel.Text = newText; });
    }
}