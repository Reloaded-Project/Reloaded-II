using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Ookii.Dialogs.Wpf;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Launcher.Misc;
using Reloaded.Mod.Loader.IO;
using Reloaded.Mod.Loader.IO.Config;
using Reloaded.Mod.Loader.IO.Structs;
using Reloaded.WPF.MVVM;
using Reloaded.WPF.Resources;

namespace Reloaded.Mod.Launcher.Models.ViewModel.Dialogs
{
    public class CreateModViewModel : ObservableObject
    {
        #region XAML Strings
        // ReSharper disable InconsistentNaming
        private const string XAML_CreateModDialogImageSelectorTitle = "CreateModDialogImageSelectorTitle";
        private const string XAML_CreateModDialogImageSelectorFilter = "CreateModDialogImageSelectorFilter";
        // ReSharper restore InconsistentNaming
        #endregion

        public IModConfig Config { get; set; } = new ModConfig();
        public ImageSource Image { get; set; } = new BitmapImage(new Uri(Paths.PLACEHOLDER_IMAGE, UriKind.Absolute));
        public ObservableCollection<BooleanGenericTuple<IModConfig>> Dependencies { get; set; } = new ObservableCollection<BooleanGenericTuple<IModConfig>>();

        public CreateModViewModel(ManageModsViewModel manageModsViewModel)
        {
            /* Build Dependencies */
            var mods = manageModsViewModel.Mods; // In case collection changes during window open.
            foreach (var mod in mods)
            {
                Dependencies.Add(new BooleanGenericTuple<IModConfig>(false, mod.ModConfig));
            }
        }

        /* Save Mod to Directory */
        public void Save()
        {
            // Make folder path and save folder.
            string modConfigDirectory = IoC.Get<LoaderConfig>().ModConfigDirectory;
            string modDirectory = Path.Combine(modConfigDirectory, PathSanitizer.ForceValidFilePath(Config.ModId));
            Directory.CreateDirectory(modDirectory);

            // Save Config
            string configSaveDirectory = Path.Combine(modDirectory, ModConfig.ConfigFileName);
            string iconSaveDirectory = Path.Combine(modDirectory, ModConfig.IconFileName);
            Config.ModIcon = ModConfig.IconFileName;
            Config.ModDependencies = Dependencies.Where(x => x.Enabled).Select(x => x.Generic.ModId).ToArray();
            Config.SupportedAppId = Constants.EmptyStringArray;

            var configLoader = new ConfigReader<ModConfig>();
            configLoader.WriteConfiguration(configSaveDirectory, (ModConfig) Config);

            // Save Image 
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create((BitmapImage) Image));
            using (FileStream stream = new FileStream(iconSaveDirectory, FileMode.OpenOrCreate))
            {
                encoder.Save(stream);
            }
        }


        /* Get Image To Display */
        public ImageSource GetImage()
        {
            string imagePath = SelectImageFile();

            if (String.IsNullOrEmpty(imagePath) || !File.Exists(imagePath))
            {
                var bitmapImage = new BitmapImage(new Uri(Paths.PLACEHOLDER_IMAGE, UriKind.Absolute));
                bitmapImage.Freeze();
                return bitmapImage;
            }

            BitmapImage source = new BitmapImage(new Uri(imagePath, UriKind.Absolute));
            source.Freeze();
            return source;
        }

        private string SelectImageFile()
        {
            var dialog = new VistaOpenFileDialog();
            dialog.Title = (string)Application.Current.Resources[XAML_CreateModDialogImageSelectorTitle];
            dialog.Filter = $"{(string)Application.Current.Resources[XAML_CreateModDialogImageSelectorFilter]} {Constants.WpfSupportedFormatsFilter}";

            if ((bool)dialog.ShowDialog())
                return dialog.FileName;

            return "";
        }
    }
}
