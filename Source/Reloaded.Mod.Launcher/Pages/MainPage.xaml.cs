using Reloaded.Mod.Launcher.Models.ViewModel;
using Reloaded.WPF.Theme.Default;

namespace Reloaded.Mod.Launcher.Pages
{
    /// <summary>
    /// The main page of the application.
    /// </summary>
    public partial class MainPage : ReloadedIIPage
    {
        private MainPageViewModel _mainPageViewModel;

        public MainPage() : base()
        {
            InitializeComponent();
            _mainPageViewModel = new MainPageViewModel();
            this.DataContext = _mainPageViewModel;
            this.AnimateInFinished += OnAnimateInFinished;
        }

        /* Preconfigured Buttons */
        private void AddApp_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _mainPageViewModel.Page = Page.AddApp;
        }

        private void OnAnimateInFinished()
        {
            // Set the title of window since splash title may still be active.
            OnLoaded(null, null);
        }
    }
}
