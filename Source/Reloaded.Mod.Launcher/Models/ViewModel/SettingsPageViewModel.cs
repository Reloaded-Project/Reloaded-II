using System.Collections.Specialized;
using System.Diagnostics;
using System.Reflection;
using Reloaded.Mod.Loader.IO;
using Reloaded.Mod.Loader.IO.Config;
using Reloaded.WPF.MVVM;

namespace Reloaded.Mod.Launcher.Models.ViewModel
{
    public class SettingsPageViewModel : ObservableObject
    {
        public MainPageViewModel MainPageViewModel { get; set; }
        public ManageModsViewModel ManageModsViewModel { get; set; }

        public int TotalApplicationsInstalled { get; set; }
        public int TotalModsInstalled { get; set; }
        public string Copyright { get; set; }
        private LoaderConfig _loaderConfig;

        public SettingsPageViewModel(MainPageViewModel mainPageViewModel, ManageModsViewModel manageModsViewModel, LoaderConfig loaderConfig)
        {
            _loaderConfig = loaderConfig;
            MainPageViewModel = mainPageViewModel;
            ManageModsViewModel = manageModsViewModel;

            UpdateTotalApplicationsInstalled();
            UpdateTotalModsInstalled();
            MainPageViewModel.ApplicationsChanged += MainPageViewModelOnApplicationsChanged;
            ManageModsViewModel.ModsChanged += ManageModsViewModelOnModsChanged;

            var version = FileVersionInfo.GetVersionInfo(Assembly.GetEntryAssembly().Location);
            Copyright = version.LegalCopyright;
        }

        public void SaveConfig() { LoaderConfigReader.WriteConfiguration(_loaderConfig); }

        /* Functions */
        private void UpdateTotalApplicationsInstalled() => TotalApplicationsInstalled = MainPageViewModel.Applications.Count;
        private void UpdateTotalModsInstalled() => TotalModsInstalled = ManageModsViewModel.Mods.Count;

        /* Events */
        private void ManageModsViewModelOnModsChanged(object sender, NotifyCollectionChangedEventArgs e) => UpdateTotalModsInstalled();
        private void MainPageViewModelOnApplicationsChanged(object sender, NotifyCollectionChangedEventArgs e) => UpdateTotalApplicationsInstalled();
    }
}
