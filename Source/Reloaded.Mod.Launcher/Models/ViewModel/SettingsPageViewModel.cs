using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
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
        public XamlFileSelector LanguageSelector => App.LanguageSelector;
        public XamlFileSelector ThemeSelector => App.ThemeSelector;

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
            Application.Current.Dispatcher.Invoke(() =>
            {
                SelectCurrentLanguage();
                SelectCurrentTheme();
            });
        }

        public void SaveConfig()
        {
            LoaderConfigReader.WriteConfiguration(LoaderConfig);
        }

        public void SaveNewLanguage()
        {
            if (LanguageSelector.File != null)
            {
                LoaderConfig.LanguageFile = LanguageSelector.File;
                SaveConfig();
            }
        }

        public void SaveNewTheme()
        {
            if (ThemeSelector.File != null)
            {
                LoaderConfig.ThemeFile = ThemeSelector.File;
                SaveConfig();

                // TODO: This is a bug workaround for where the language ComboBox gets reset after a theme change.
                SelectCurrentLanguage();
            }
        }

        public void SelectCurrentLanguage() => SelectXamlFile(LanguageSelector, LoaderConfig.LanguageFile);
        public void SelectCurrentTheme()    => SelectXamlFile(ThemeSelector, LoaderConfig.ThemeFile);

        private void SelectXamlFile(XamlFileSelector selector, string fileName)
        {
            foreach (var file in selector.Files)
            {
                if (file != fileName) 
                    continue;
                
                selector.File = file;
                break;
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
