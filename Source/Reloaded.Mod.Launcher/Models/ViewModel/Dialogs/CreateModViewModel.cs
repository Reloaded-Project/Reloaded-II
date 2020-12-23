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
using Reloaded.Mod.Loader.IO.Structs;
using Reloaded.Mod.Loader.IO.Utility;
using Reloaded.WPF.Utilities;
using ObservableObject = Reloaded.WPF.MVVM.ObservableObject;

namespace Reloaded.Mod.Launcher.Models.ViewModel.Dialogs
{
    public class CreateModViewModel : ObservableObject
    {
        public IModConfig Config { get; set; } = new ModConfig();
        public ImageSource Image { get; set; } = new BitmapImage(Constants.PlaceholderImagePath);
        public ObservableCollection<BooleanGenericTuple<IModConfig>> Dependencies { get; set; } = new ObservableCollection<BooleanGenericTuple<IModConfig>>();
        public string ModsFilter { get; set; } = "";

        private XamlResource<string> _xamlCreateModDialogSelectorTitle = new XamlResource<string>("CreateModDialogImageSelectorTitle");
        private XamlResource<string> _xamlCreateModDialogSelectorFilter = new XamlResource<string>("CreateModDialogImageSelectorFilter");
        private readonly CollectionViewSource _dependenciesViewSource;

        public CreateModViewModel(ManageModsViewModel manageModsViewModel, DictionaryResourceManipulator manipulator)
        {
            /* Build Dependencies */
            var mods = manageModsViewModel.Mods; // In case collection changes during window open.
            foreach (var mod in mods)
            {
                Dependencies.Add(new BooleanGenericTuple<IModConfig>(false, mod.ModConfig));
            }

            _dependenciesViewSource = manipulator.Get<CollectionViewSource>("SortedDependencies");
            _dependenciesViewSource.Filter += DependenciesViewSourceOnFilter;
        }

        /* Save Mod to Directory */
        public void Save()
        {
            // Make folder path and save folder.
            string modConfigDirectory = IoC.Get<LoaderConfig>().ModConfigDirectory;
            string modDirectory = Path.Combine(modConfigDirectory, IOEx.ForceValidFilePath(Config.ModId));
            Directory.CreateDirectory(modDirectory);

            // Save Config
            string configSaveDirectory = Path.Combine(modDirectory, ModConfig.ConfigFileName);
            string iconSaveDirectory = Path.Combine(modDirectory, ModConfig.IconFileName);
            Config.ModIcon = ModConfig.IconFileName;
            Config.ModDependencies = Dependencies.Where(x => x.Enabled).Select(x => x.Generic.ModId).ToArray();
            Config.SupportedAppId = Constants.EmptyStringArray;

            ConfigReader<ModConfig>.WriteConfiguration(configSaveDirectory, (ModConfig) Config);

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
