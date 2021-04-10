using System.Collections.Specialized;
using System.Diagnostics;
using System.Threading.Tasks;
using Reloaded.Mod.Launcher.Utility;
using Reloaded.Mod.Loader.IO;
using Reloaded.Mod.Loader.IO.Config;
using Reloaded.Mod.Loader.IO.Services;
using Reloaded.WPF.MVVM;

namespace Reloaded.Mod.Launcher.Models.ViewModel
{
    public class SettingsPageViewModel : ObservableObject
    {
        public ApplicationConfigService AppConfigService { get; set; }
        public ModConfigService ModConfigService { get; set; }

        public int TotalApplicationsInstalled { get; set; }
        public int TotalModsInstalled { get; set; }
        public string Copyright { get; set; }
        public string RuntimeVersion { get; set; }
        public LoaderConfig LoaderConfig { get; set; }
        public XamlFileSelector LanguageSelector => App.LanguageSelector;
        public XamlFileSelector ThemeSelector => App.ThemeSelector;

        public SettingsPageViewModel(ApplicationConfigService appConfigService, ModConfigService modConfigService, LoaderConfig loaderConfig)
        {
            AppConfigService = appConfigService;
            ModConfigService = modConfigService;
            LoaderConfig = loaderConfig;

            UpdateTotalApplicationsInstalled();
            UpdateTotalModsInstalled();
            AppConfigService.Applications.CollectionChanged += MainPageViewModelOnApplicationsChanged;
            ModConfigService.Mods.CollectionChanged += ManageModsViewModelOnModsChanged;

            var version = FileVersionInfo.GetVersionInfo(Process.GetCurrentProcess().MainModule.FileName);
            Copyright = version.LegalCopyright;
            RuntimeVersion = $"Core: {System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription}";
            ActionWrappers.ExecuteWithApplicationDispatcher(() =>
            {
                SelectCurrentLanguage();
                SelectCurrentTheme();
            });
        }

        public async Task SaveConfigAsync()
        {
            await IConfig<LoaderConfig>.ToPathAsync(LoaderConfig, Paths.LoaderConfigPath);
        }

        public async Task SaveNewLanguage()
        {
            if (LanguageSelector.File != null)
            {
                LoaderConfig.LanguageFile = LanguageSelector.File;
                await SaveConfigAsync();
            }
        }

        public async Task SaveNewTheme()
        {
            if (ThemeSelector.File != null)
            {
                LoaderConfig.ThemeFile = ThemeSelector.File;
                await SaveConfigAsync();

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
        private void UpdateTotalApplicationsInstalled() => TotalApplicationsInstalled = AppConfigService.Applications.Count;
        private void UpdateTotalModsInstalled() => TotalModsInstalled = ModConfigService.Mods.Count;

        /* Events */
        private void ManageModsViewModelOnModsChanged(object sender, NotifyCollectionChangedEventArgs e) => UpdateTotalModsInstalled();
        private void MainPageViewModelOnApplicationsChanged(object sender, NotifyCollectionChangedEventArgs e) => UpdateTotalApplicationsInstalled();
    }
}
