using System.ComponentModel;
using System.Windows;
using Reloaded.Mod.Launcher.Commands.Dialog;
using Reloaded.Mod.Launcher.Models.ViewModel;

namespace Reloaded.Mod.Launcher.Pages.BaseSubpages
{
    /// <summary>
    /// Interaction logic for SettingsPage.xaml
    /// </summary>
    public partial class SettingsPage : ReloadedIIPage
    {
        public SettingsPageViewModel ViewModel { get; set; }

        public SettingsPage()
        {
            InitializeComponent();
            ViewModel = IoC.Get<SettingsPageViewModel>();
            this.AnimateOutStarted += OnLeavingPage;
            IoC.Get<MainWindow>().Closing += OnMainWindowExit;
        }

        private void OnMainWindowExit(object sender, CancelEventArgs e) => OnLeavingPage();
        private void OnLeavingPage() => ViewModel.SaveConfig();
        private void Documents_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e) => new OpenDocumentationCommand().Execute(null);
        private void LanguageChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            ViewModel.SaveNewLanguage();
        }
    }
}
