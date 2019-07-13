using System.Windows;
using Reloaded.Mod.Launcher.Commands.Dialog;
using Reloaded.Mod.Launcher.Models.ViewModel;
using Reloaded.Mod.Launcher.Pages.Dialogs;
using Reloaded.Mod.Loader.IO;

namespace Reloaded.Mod.Launcher.Pages.BaseSubpages
{
    /// <summary>
    /// Interaction logic for SettingsPage.xaml
    /// </summary>
    public partial class SettingsPage : ReloadedIIPage
    {
        #region XAML_Constants
        // ReSharper disable InconsistentNaming
        public const string XAML_LoaderSettingsCleanup = "LoaderSettingsCleanup";
        public const string XAML_LoaderSettingsCleanupWarning = "LoaderSettingsCleanupWarning";
        // ReSharper restore InconsistentNaming
        #endregion

        public SettingsPageViewModel ViewModel { get; set; }

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
            var messageBox = new MessageBoxOkCancel((string)Application.Current.Resources[XAML_LoaderSettingsCleanup],
                                                    (string)Application.Current.Resources[XAML_LoaderSettingsCleanupWarning]);
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
