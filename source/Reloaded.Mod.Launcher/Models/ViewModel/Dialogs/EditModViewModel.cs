using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Ookii.Dialogs.Wpf;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Launcher.Misc;
using Reloaded.Mod.Loader.IO;
using Reloaded.Mod.Loader.IO.Config;
using Reloaded.Mod.Loader.IO.Services;
using Reloaded.Mod.Loader.IO.Structs;
using Reloaded.WPF.Utilities;
using ObservableObject = Reloaded.WPF.MVVM.ObservableObject;

namespace Reloaded.Mod.Launcher.Models.ViewModel.Dialogs
{
    public class EditModViewModel : ObservableObject
    {
        public ModConfig Config { get; set; }

        public ImageSource Image { get; set; } = new BitmapImage(Constants.PlaceholderImagePath);

        public ObservableCollection<BooleanGenericTuple<IModConfig>> Dependencies { get; set; } = new ObservableCollection<BooleanGenericTuple<IModConfig>>();

        public string ModsFilter { get; set; } = "";

        private XamlResource<string> _xamlCreateModDialogSelectorTitle = new XamlResource<string>("CreateModDialogImageSelectorTitle");
        private XamlResource<string> _xamlCreateModDialogSelectorFilter = new XamlResource<string>("CreateModDialogImageSelectorFilter");
        private readonly CollectionViewSource _dependenciesViewSource;

        private PathTuple<ModConfig> _modTuple;

        public EditModViewModel(PathTuple<ModConfig> modTuple, ModConfigService modConfigService, DictionaryResourceManipulator manipulator)
        {
            _modTuple = modTuple;
            Config = _modTuple.Config;
            if (modTuple.Config.TryGetIconPath(modTuple.Path, out var iconPath))
                Image = Imaging.BitmapFromUri(new Uri(iconPath));

            /* Build Dependencies */
            var mods = modConfigService.Items; // In case collection changes during window open.
            foreach (var mod in mods)
                Dependencies.Add(new BooleanGenericTuple<IModConfig>(false, mod.Config));

            _dependenciesViewSource = manipulator.Get<CollectionViewSource>("SortedDependencies");
            _dependenciesViewSource.Filter += DependenciesViewSourceOnFilter;
        }

        /* Save Mod to Directory */
        public void Save()
        {
            // Make folder path and save folder.
            string modDirectory = Path.GetDirectoryName(_modTuple.Path);

            // Save Config
            string configSavePath  = Path.Combine(modDirectory, ModConfig.ConfigFileName);
            string iconSavePath    = Path.Combine(modDirectory, ModConfig.IconFileName);
            Config.ModIcon         = ModConfig.IconFileName;
            Config.ModDependencies = Dependencies.Where(x => x.Enabled).Select(x => x.Generic.ModId).ToArray();

            ConfigReader<ModConfig>.WriteConfiguration(configSavePath, (ModConfig) Config);

            // Save Image 
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create((BitmapImage) Image));
            using FileStream stream = new FileStream(iconSavePath, FileMode.OpenOrCreate);
            encoder.Save(stream);
        }

        /* Get Image To Display */
        public ImageSource GetImage()
        {
            string imagePath = SelectImageFile();

            if (String.IsNullOrEmpty(imagePath) || !File.Exists(imagePath))
            {
                var bitmapImage = new BitmapImage(Constants.PlaceholderImagePath);
                bitmapImage.Freeze();
                return bitmapImage;
            }

            BitmapImage source = new BitmapImage(new Uri(imagePath, UriKind.Absolute));
            source.Freeze();
            return source;
        }

        public void RefreshModList() => _dependenciesViewSource.View.Refresh();

        private void DependenciesViewSourceOnFilter(object sender, FilterEventArgs e)
        {
            if (this.ModsFilter.Length <= 0)
            {
                e.Accepted = true;
                return;
            }

            var tuple = (BooleanGenericTuple<IModConfig>)e.Item;
            e.Accepted = tuple.Generic.ModName.IndexOf(this.ModsFilter, StringComparison.InvariantCultureIgnoreCase) >= 0;
        }

        private string SelectImageFile()
        {
            var dialog = new VistaOpenFileDialog();
            dialog.Title = _xamlCreateModDialogSelectorTitle.Get();
            dialog.Filter = $"{_xamlCreateModDialogSelectorFilter.Get()} {Constants.WpfSupportedFormatsFilter}";

            if ((bool)dialog.ShowDialog())
                return dialog.FileName;

            return "";
        }
    }
}
