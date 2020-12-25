using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Reloaded.Mod.Launcher.Models.Model;
using Reloaded.Mod.Loader.IO.Config;
using Reloaded.Mod.Loader.IO.Structs;
using Reloaded.WPF.MVVM;
using Reloaded.WPF.Theme.Default;

namespace Reloaded.Mod.Launcher.Pages.Dialogs
{
    /// <summary>
    /// Interaction logic for IncompatibleModDialog.xaml
    /// </summary>
    public partial class IncompatibleModDialog : ReloadedWindow
    {
        public new IncompatibleModArchiveViewmodel ViewModel { get; set; }

        public IncompatibleModDialog(IEnumerable<ImageModPathTuple> modConfigs, PathTuple<ApplicationConfig> config)
        {
            InitializeComponent();
            ViewModel = new IncompatibleModArchiveViewmodel(modConfigs, config);
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.EnableMods();
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.DisableMods();
            Close();
        }
    }

    public class IncompatibleModArchiveViewmodel : ObservableObject
    {
        public PathTuple<ApplicationConfig> ApplicationConfig { get; set; }
        public IEnumerable<ImageModPathTuple> Mods { get; set; }

        /* Setup & Teardown */
        public IncompatibleModArchiveViewmodel(IEnumerable<ImageModPathTuple> modConfigs, PathTuple<ApplicationConfig> applicationConfig)
        {
            Mods = modConfigs;
            ApplicationConfig = applicationConfig;
        }

        /// <summary>
        /// Disables all mods for the application.
        /// </summary>
        public void DisableMods()
        {
            var enabledModHashset = ApplicationConfig.Config.EnabledMods.ToHashSet();
            var removeModHashset  = Mods.Select(x => x.ModConfig.ModId).ToHashSet();
            enabledModHashset.ExceptWith(removeModHashset);

            ApplicationConfig.Config.EnabledMods = enabledModHashset.ToArray();
            ApplicationConfig.Save();
        }

        /// <summary>
        /// Marks all mods compatible with the application.
        /// </summary>
        public void EnableMods()
        {
            foreach (var mod in Mods)
            {
                var supportedAppIds = new List<string>(mod.ModConfig.SupportedAppId) { ApplicationConfig.Config.AppId };
                mod.ModConfig.SupportedAppId = supportedAppIds.ToArray();
                mod.Save();
            }
        }
    }
}
