using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
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
        public string RuntimeVersion { get; set; }
        public LoaderConfig LoaderConfig { get; set; }
        public XamlFileSelector LanguageSelector => App.Selector;

        public SettingsPageViewModel(MainPageViewModel mainPageViewModel, ManageModsViewModel manageModsViewModel, LoaderConfig loaderConfig)
        {
            LoaderConfig = loaderConfig;
            MainPageViewModel = mainPageViewModel;
            ManageModsViewModel = manageModsViewModel;

            UpdateTotalApplicationsInstalled();
            UpdateTotalModsInstalled();
            MainPageViewModel.ApplicationsChanged += MainPageViewModelOnApplicationsChanged;
            ManageModsViewModel.ModsChanged += ManageModsViewModelOnModsChanged;

            var version = FileVersionInfo.GetVersionInfo(Process.GetCurrentProcess().MainModule.FileName);
            Copyright = version.LegalCopyright;
            RuntimeVersion = $"Core: {System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription}";
            SelectCurrentLanguage();
        }

        public void SaveConfig()
        {
            LoaderConfigReader.WriteConfiguration(LoaderConfig);
        }

        public void SaveNewLanguage()
        {
            LoaderConfig.LanguageFile = LanguageSelector.File;
            SaveConfig();
        }

        public void SelectCurrentLanguage()
        {
            for (int x = 0; x < LanguageSelector.Files.Count; x++)
            {
                if (LanguageSelector.Files[x] == LoaderConfig.LanguageFile)
                {
                    LanguageSelector.File = LanguageSelector.Files[x];
                    break;
                }
            }
        }

        /* Functions */
        private void UpdateTotalApplicationsInstalled() => TotalApplicationsInstalled = MainPageViewModel.Applications.Count;
        private void UpdateTotalModsInstalled() => TotalModsInstalled = ManageModsViewModel.Mods.Count;

        /* Events */
        private void ManageModsViewModelOnModsChanged(object sender, NotifyCollectionChangedEventArgs e) => UpdateTotalModsInstalled();
        private void MainPageViewModelOnApplicationsChanged(object sender, NotifyCollectionChangedEventArgs e) => UpdateTotalApplicationsInstalled();
    }
}
