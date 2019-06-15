using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using System.Windows;
using Reloaded.Mod.Launcher.Models.ViewModel;
using Reloaded.Mod.Launcher.Pages.Dialogs;
using Reloaded.Mod.Loader.IO;
using WindowViewModel = Reloaded.Mod.Launcher.Models.ViewModel.WindowViewModel;

namespace Reloaded.Mod.Launcher.Pages
{
    /// <summary>
    /// The main page of the application.
    /// </summary>
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public partial class SplashPage : ReloadedIIPage
    {
        private const string XAML_SplashMinimumTime = "SplashMinimumTime";
        private SplashViewModel _splashViewModel;
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
                Task.Run(() => Setup.SetupApplication(GetText, UpdateText, (int)Application.Current.Resources[XAML_SplashMinimumTime]))
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
                var loaderConfig = LoaderConfigReader.ReadConfiguration();
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
        /// Retrieves text from XAML resources given a specific argument.
        /// </summary>
        private string GetText(string arg)
        {
            return (string) Application.Current.Resources[arg];
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
