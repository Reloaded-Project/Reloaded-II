using System;
using System.Windows.Media.Animation;
using Reloaded.Mod.Launcher.Models.ViewModel;
using Reloaded.Mod.Launcher.Pages.BaseSubpages;
using Reloaded.WPF.Theme.Default;

namespace Reloaded.Mod.Launcher.Pages
{
    /// <summary>
    /// The main page of the application.
    /// </summary>
    public partial class BasePage : ReloadedIIPage
    {
        private MainPageViewModel _mainPageViewModel;

        public BasePage() : base()
        {
            InitializeComponent();
            _mainPageViewModel = IoC.Get<MainPageViewModel>();
            this.DataContext = _mainPageViewModel;
            this.AnimateInFinished += OnAnimateInFinished;
        }

        /* Preconfigured Buttons */
        private void AddApp_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _mainPageViewModel.Page = BaseSubPage.AddApp;
        }
    }
}
