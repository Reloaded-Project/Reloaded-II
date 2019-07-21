using System.Windows;
using Reloaded.Mod.Launcher.Commands.Dialog;
using Reloaded.Mod.Launcher.Models.ViewModel;
using Reloaded.Mod.Launcher.Pages.Dialogs;
using Reloaded.Mod.Launcher.Utility;
using Reloaded.Mod.Loader.IO;
using Reloaded.WPF.Utilities;

namespace Reloaded.Mod.Launcher.Pages.BaseSubpages
{
    /// <summary>
    /// Interaction logic for SettingsPage.xaml
    /// </summary>
    public partial class SettingsPage : ReloadedIIPage
    {
        public SettingsPageViewModel ViewModel { get; set; }
        private XamlResource<string> _xamlLoaderSettingsCleanup = new XamlResource<string>("LoaderSettingsCleanup");
        private XamlResource<string> _xamlLoaderSettingsCleanupWarning = new XamlResource<string>("LoaderSettingsCleanupWarning");

        public SettingsPage()
        {
            InitializeComponent();
            ViewModel = IoC.Get<SettingsPageViewModel>();
            this.AnimateOutStarted += OnAnimateOutStarted;
        }

        private void OnAnimateOutStarted()
        {
            LoaderConfigReader.WriteConfiguration(ViewModel.LoaderConfig);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var messageBox = new MessageBoxOkCancel(_xamlLoaderSettingsCleanup.Get(),
                                                    _xamlLoaderSettingsCleanupWarning.Get());
            messageBox.Owner = Window.GetWindow(this);
            messageBox.ShowDialog();

            if (messageBox.DialogResult.HasValue && messageBox.DialogResult.Value)
            {
                var configCleaner = IoC.Get<ConfigCleaner>();
                configCleaner.Clean();
            }
        }

        private void Documents_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            new OpenDocumentationCommand().Execute(null);
        }
    }
}
