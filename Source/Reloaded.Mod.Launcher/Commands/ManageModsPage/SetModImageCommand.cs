using System;
using System.IO;
using System.Windows.Input;
using System.Windows.Media;
using Ookii.Dialogs.Wpf;
using Reloaded.Mod.Launcher.Misc;
using Reloaded.Mod.Launcher.Models.ViewModel;
using Reloaded.Mod.Loader.IO.Config;
using Reloaded.Mod.Loader.IO.Structs;
using Reloaded.WPF.Utilities;

namespace Reloaded.Mod.Launcher.Commands.ManageModsPage
{
    /// <summary>
    /// Command to be used by the <see cref="AddAppPage"/> which allows
    /// for the addition of a new application.
    /// </summary>
    public class SetModImageCommand : ICommand
    {
        private XamlResource<string> _xamlCreateModDialogSelectorTitle = new XamlResource<string>("CreateModDialogImageSelectorTitle");
        private XamlResource<string> _xamlCreateModDialogSelectorFilter = new XamlResource<string>("CreateModDialogImageSelectorFilter");
        private readonly ManageModsViewModel _manageModsViewModel;

        public SetModImageCommand()
        {
            _manageModsViewModel = IoC.Get<ManageModsViewModel>();
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
            string modDirectory = Path.GetDirectoryName(modTuple.ModConfigPath);

            // Set icon name and save.
            string iconFileName = Path.GetFileName(imagePath);
            if (modDirectory != null)
            {
                string iconPath = Path.Combine(modDirectory, iconFileName);

                // Copy image and set config file path.
                File.Copy(imagePath, iconPath, true);
                modTuple.ModConfig.ModIcon = iconFileName;

                // No need to write file on disk, file will be updated by binding.
                ImageSource source = Imaging.BitmapFromUri(new Uri(iconPath, UriKind.Absolute));
                _manageModsViewModel.Icon = source;
                modTuple.Image = _manageModsViewModel.GetImageForModConfig(new PathGenericTuple<ModConfig>(modTuple.ModConfigPath, (ModConfig) modTuple.ModConfig));
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
