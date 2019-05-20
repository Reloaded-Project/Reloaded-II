using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Ookii.Dialogs.Wpf;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Launcher.Models.ViewModel;
using Reloaded.Mod.Loader.IO;
using Reloaded.Mod.Loader.IO.Config;

namespace Reloaded.Mod.Launcher.Commands
{
    /// <summary>
    /// Comnmand to be used by the <see cref="AddAppPage"/> which allows
    /// for the addition of a new application.
    /// </summary>
    public class SetApplicationImageCommand : ICommand
    {
        // ReSharper disable InconsistentNaming
        private const string XAML_AddAppImageExecutableTitle = "AddAppImageSelectorTitle";
        private const string XAML_AddAppImageSelectorFilter = "AddAppImageSelectorFilter";
        // ReSharper restore InconsistentNaming

        private AddAppViewModel _addAppViewModel;

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
            string applicationIconPath = Path.Combine(applicationDirectory, applicationIconFileName);

            // Copy image and set config file path.
            File.Copy(imagePath, applicationIconPath);
            config.AppIcon = applicationIconFileName;

            // No need to write file on disk, file will be updated by image.
            ImageSource source = new BitmapImage(new Uri(applicationIconPath, UriKind.Absolute));
            source.Freeze();
            appIconPathTuple.Image = source;
        }

        public event EventHandler CanExecuteChanged = (sender, args) => { };

        /// <summary>
        /// Opens up a file selection dialog allowing for the selection of a custom image to associate with the profile.
        /// </summary>
        private string SelectImageFile()
        {
            var dialog = new VistaOpenFileDialog();
            dialog.Title = (string) Application.Current.Resources[XAML_AddAppImageExecutableTitle];
            dialog.Filter = $"{(string)Application.Current.Resources[XAML_AddAppImageSelectorFilter]} (*.jpg, *.jpeg, *.jpe, *.jfif, *.png)|*.jpg; *.jpeg; *.jpe; *.jfif; *.png";
            if ((bool) dialog.ShowDialog())
            {
                return dialog.FileName;
            }

            return "";
        }
    }
}
