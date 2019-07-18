using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Ookii.Dialogs.Wpf;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Launcher.Misc;
using Reloaded.Mod.Launcher.Models.ViewModel;
using Reloaded.Mod.Launcher.Utility;

namespace Reloaded.Mod.Launcher.Commands.AddAppPage
{
    /// <summary>
    /// Command to be used by the <see cref="AddAppPage"/> which allows
    /// for the addition of a new application.
    /// </summary>
    public class SetApplicationImageCommand : ICommand
    {
        // ReSharper disable InconsistentNaming
        private const string XAML_AddAppImageExecutableTitle = "AddAppImageSelectorTitle";
        private const string XAML_AddAppImageSelectorFilter = "AddAppImageSelectorFilter";
        // ReSharper restore InconsistentNaming

        private readonly AddAppViewModel _addAppViewModel;

        public SetApplicationImageCommand()
        {
            _addAppViewModel = IoC.Get<AddAppViewModel>();
        }

        public bool CanExecute(object parameter)
        {
            if (_addAppViewModel.SelectedIndex != -1 && _addAppViewModel.Application != null)
                return true;

            return false;
        }

        public void Execute(object parameter)
        {
            // Select image
            string imagePath = SelectImageFile();

            if (String.IsNullOrEmpty(imagePath) || ! File.Exists(imagePath))
                return;

            // Get current selected application and its paths.
            IApplicationConfig config = _addAppViewModel.Application;

            // Get application entry in set of all applications.
            var appIconPathTuple = _addAppViewModel.MainPageViewModel.Applications.First( x => x.ApplicationConfig.Equals(config) );
            string applicationDirectory = Path.GetDirectoryName(appIconPathTuple.ApplicationConfigPath);

            string applicationIconFileName = Path.GetFileName(imagePath);
            if (applicationDirectory != null)
            {
                string applicationIconPath = Path.Combine(applicationDirectory, applicationIconFileName);

                // Copy image and set config file path.
                File.Copy(imagePath, applicationIconPath, true);
                config.AppIcon = applicationIconFileName;

                // No need to write file on disk, file will be updated by binding.
                ImageSource source = Imaging.BitmapFromUri(new Uri(applicationIconPath, UriKind.Absolute));
                appIconPathTuple.Image = source;
            }
        }

        public event EventHandler CanExecuteChanged = (sender, args) => { };

        /// <summary>
        /// Opens up a file selection dialog allowing for the selection of a custom image to associate with the profile.
        /// </summary>
        private string SelectImageFile()
        {
            var dialog = new VistaOpenFileDialog();
            dialog.Title = ApplicationResourceAcquirer.GetTypeOrDefault<string>(XAML_AddAppImageExecutableTitle);
            dialog.Filter = $"{ApplicationResourceAcquirer.GetTypeOrDefault<string>(XAML_AddAppImageSelectorFilter)} {Constants.WpfSupportedFormatsFilter}";
            if ((bool) dialog.ShowDialog())
            {
                return dialog.FileName;
            }

            return "";
        }
    }
}
