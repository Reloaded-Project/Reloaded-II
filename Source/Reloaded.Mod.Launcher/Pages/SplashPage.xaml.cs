using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using System.Windows;
using Reloaded.Mod.Launcher.Models.ViewModel;
using Reloaded.Mod.Launcher.Pages.Dialogs;
using Reloaded.Mod.Launcher.Utility;
using Reloaded.Mod.Loader.IO;
using Reloaded.Mod.Loader.IO.Config;
using Reloaded.WPF.Utilities;
using WindowViewModel = Reloaded.Mod.Launcher.Models.ViewModel.WindowViewModel;

namespace Reloaded.Mod.Launcher.Pages
{
    /// <summary>
    /// The main page of the application.
    /// </summary>
    public partial class SplashPage : ReloadedIIPage
    {
        private readonly SplashViewModel _splashViewModel;
        private XamlResource<int> _xamlSplashMinimumTime = new XamlResource<int>("SplashMinimumTime");
        private bool _loaded = false;

        public SplashPage() : base()
        {  
            InitializeComponent();
            this.Loaded += AfterLoading;

            // Setup ViewModel
            _splashViewModel = new SplashViewModel();
            this.DataContext = _splashViewModel;
        }

        private void AfterLoading(object sender, RoutedEventArgs e)
        {
            // Start preparing everything on Splash Screen!
            if (!_loaded)
            {
                _loaded = true;
                var task = Task.Run(() => Setup.SetupApplication(UpdateText, _xamlSplashMinimumTime.Get()))
                            .ContinueWith(ChangeToMainPage)
                            .ContinueWith(DisplayFirstLaunchWarningIfNeeded);
            }
        }

        private void ChangeToMainPage(Task obj)
        {
            IoC.Get<MainWindow>().Dispatcher.Invoke(() =>
            {
                var viewModel = IoC.Get<WindowViewModel>();
                viewModel.CurrentPage = Page.Base;
            });
        }

        private void DisplayFirstLaunchWarningIfNeeded(Task obj)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var loaderConfig = IoC.Get<LoaderConfig>();
                if (loaderConfig.FirstLaunch)
                {
                    var firstLaunchWindow = new FirstLaunch();
                    firstLaunchWindow.Owner = Window.GetWindow(this);
                    firstLaunchWindow.ShowDialog();
                    loaderConfig.FirstLaunch = false;
                    LoaderConfigReader.WriteConfiguration(loaderConfig);
                }

            });
        }

        /// <summary>
        /// Runs a method intended to update the UI thread.
        /// </summary>
        private void UpdateText(string newText)
        {
            this.Dispatcher.Invoke(() => { _splashViewModel.Text = newText; });
        }
    }
}
