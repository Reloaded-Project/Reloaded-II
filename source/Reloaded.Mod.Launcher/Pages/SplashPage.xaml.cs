using System;
using System.Threading.Tasks;
using System.Windows;
using Reloaded.Mod.Launcher.Models.ViewModel;
using Reloaded.Mod.Launcher.Pages.Dialogs;
using Reloaded.Mod.Launcher.Utility;
using Reloaded.Mod.Loader.IO;
using Reloaded.Mod.Loader.IO.Config;
using Reloaded.WPF.Utilities;
using MessageBox = Reloaded.Mod.Launcher.Pages.Dialogs.MessageBox;
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
        private XamlResource<string> _xamlFailedToLaunchTitle = new XamlResource<string>("FailedToLaunchTitle");
        private XamlResource<string> _xamlFailedToLaunchMessage = new XamlResource<string>("FailedToLaunchMessage");
        private Task _setupApplicationTask;
        
        public SplashPage() : base()
        {  
            InitializeComponent();

            // Setup ViewModel
            _splashViewModel    = new SplashViewModel();
            this.DataContext    = _splashViewModel;
            _setupApplicationTask = Task.Run(() => Setup.SetupApplicationAsync(UpdateText, _xamlSplashMinimumTime.Get()));
            this.Loaded        += (a, b) => ActionWrappers.ExecuteWithApplicationDispatcher(Load);
        }

        private async void Load()
        {
            try
            {
                await _setupApplicationTask;
                ChangeToMainPage();
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
            var viewModel = IoC.Get<WindowViewModel>();
            viewModel.CurrentPage = Page.Base;
        }

        private void DisplayFirstLaunchWarningIfNeeded()
        {
            var loaderConfig = IoC.Get<LoaderConfig>();
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
            ActionWrappers.ExecuteWithApplicationDispatcher(() => { _splashViewModel.Text = newText; });
        }
    }
}
