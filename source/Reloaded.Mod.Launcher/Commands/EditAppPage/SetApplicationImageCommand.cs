using System;
using System.IO;
using System.Windows.Input;
using Ookii.Dialogs.Wpf;
using Reloaded.Mod.Launcher.Misc;
using Reloaded.Mod.Launcher.Models.ViewModel;
using Reloaded.WPF.Utilities;

namespace Reloaded.Mod.Launcher.Commands.EditAppPage
{
    /// <summary>
    /// Command to be used by the <see cref="EditAppPage"/> which allows
    /// for the addition of a new application.
    /// </summary>
    public class SetApplicationImageCommand : ICommand
    {
        private XamlResource<string> _xamlAddAppImageExecutableTitle    = new XamlResource<string>("AddAppImageSelectorTitle");
        private XamlResource<string> _xamlAddAppImageSelectorFilter     = new XamlResource<string>("AddAppImageSelectorFilter");
        private readonly EditAppViewModel _editAppViewModel;

        public SetApplicationImageCommand(EditAppViewModel model)
        {
            _editAppViewModel = model;
        }

        public bool CanExecute(object parameter)
        {
            if (_editAppViewModel.Application != null)
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
            var application = _editAppViewModel.Application;
            string applicationDirectory = Path.GetDirectoryName(application.Path);

            // Get application entry in set of all applications.
            string applicationIconFileName = Path.GetFileName(imagePath);
            if (applicationDirectory != null)
            {
                string applicationIconPath = Path.Combine(applicationDirectory, applicationIconFileName);

                // Copy image and set config file path.
                File.Copy(imagePath, applicationIconPath, true);
                application.Config.AppIcon = applicationIconFileName;
                application.Save();
            }
        }

        public event EventHandler CanExecuteChanged = (sender, args) => { };

        /// <summary>
        /// Opens up a file selection dialog allowing for the selection of a custom image to associate with the profile.
        /// </summary>
        private string SelectImageFile()
        {
            var dialog = new VistaOpenFileDialog();
            dialog.Title = _xamlAddAppImageExecutableTitle.Get();
            dialog.Filter = $"{_xamlAddAppImageSelectorFilter.Get()} {Constants.WpfSupportedFormatsFilter}";
            if ((bool) dialog.ShowDialog())
            {
                return dialog.FileName;
            }

            return "";
        }
    }
}
