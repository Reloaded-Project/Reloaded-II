using System;
using System.IO;
using System.Windows.Input;
using Ookii.Dialogs.Wpf;
using Reloaded.Mod.Launcher.Misc;
using Reloaded.Mod.Launcher.Models.ViewModel;
using Reloaded.WPF.Utilities;

namespace Reloaded.Mod.Launcher.Commands.ManageModsPage
{
    /// <summary>
    /// Command to be used by the <see cref="EditAppPage"/> which allows
    /// for the addition of a new application.
    /// </summary>
    public class SetModImageCommand : ICommand
    {
        private XamlResource<string> _xamlCreateModDialogSelectorTitle = new XamlResource<string>("CreateModDialogImageSelectorTitle");
        private XamlResource<string> _xamlCreateModDialogSelectorFilter = new XamlResource<string>("CreateModDialogImageSelectorFilter");
        private readonly ManageModsViewModel _manageModsViewModel;

        public SetModImageCommand(ManageModsViewModel manageModsViewModel)
        {
            _manageModsViewModel = manageModsViewModel;
        }

        public bool CanExecute(object parameter)
        {
            if (_manageModsViewModel.SelectedModTuple != null)
                return true;

            return false;
        }

        public void Execute(object parameter)
        {
            // Select image
            string imagePath = SelectImageFile();

            if (String.IsNullOrEmpty(imagePath) || ! File.Exists(imagePath))
                return;

            // Get selected item.
            var modTuple = _manageModsViewModel.SelectedModTuple;
            string modDirectory = Path.GetDirectoryName(modTuple.Path);

            // Set icon name and save.
            string iconFileName = Path.GetFileName(imagePath);
            if (modDirectory != null)
            {
                string iconPath = Path.Combine(modDirectory, iconFileName);

                // Copy image and set config file path.
                File.Copy(imagePath, iconPath, true);
                modTuple.Config.ModIcon = iconFileName;
            }
        }

        public event EventHandler CanExecuteChanged = (sender, args) => { };

        /// <summary>
        /// Opens up a file selection dialog allowing for the selection of a custom image to associate with the profile.
        /// </summary>
        private string SelectImageFile()
        {
            var dialog = new VistaOpenFileDialog();
            dialog.Title = _xamlCreateModDialogSelectorTitle.Get();
            dialog.Filter = $"{_xamlCreateModDialogSelectorFilter.Get()} {Constants.WpfSupportedFormatsFilter}";
            if ((bool) dialog.ShowDialog())
            {
                return dialog.FileName;
            }

            return "";
        }
    }
}
