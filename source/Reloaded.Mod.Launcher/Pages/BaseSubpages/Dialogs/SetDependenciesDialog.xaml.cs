using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Data;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Loader.IO.Config;
using Reloaded.Mod.Loader.IO.Services;
using Reloaded.Mod.Loader.IO.Structs;
using Reloaded.Mod.Loader.IO.Utility;
using Reloaded.WPF.Theme.Default;
using Reloaded.WPF.Utilities;

namespace Reloaded.Mod.Launcher.Pages.BaseSubpages.Dialogs
{
    /// <summary>
    /// Interaction logic for SetDependenciesDialog.xaml
    /// </summary>
    public partial class SetDependenciesDialog : ReloadedWindow
    {
        public new SetDependenciesDialogViewmodel ViewModel { get; set; }
        private readonly CollectionViewSource _dependenciesViewSource;

        public SetDependenciesDialog(ModConfigService modConfigService, PathTuple<ModConfig> config)
        {
            InitializeComponent();
            ViewModel = new SetDependenciesDialogViewmodel(modConfigService, config);

            var manipulator = new DictionaryResourceManipulator(this.Contents.Resources);
            _dependenciesViewSource = manipulator.Get<CollectionViewSource>("SortedDependencies");
            _dependenciesViewSource.Filter += DependenciesViewSourceOnFilter;

            this.Closed += OnClosed;
        }

        private void OnClosed(object sender, EventArgs e) => ViewModel.SaveCurrentMod();
        private void ModsFilter_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e) => _dependenciesViewSource.View.Refresh();

        private void DependenciesViewSourceOnFilter(object sender, FilterEventArgs e)
        {
            if (ViewModel.ModsFilter.Length <= 0)
            {
                e.Accepted = true;
                return;
            }

            var tuple = (BooleanGenericTuple<IModConfig>)e.Item;
            e.Accepted = tuple.Generic.ModName.Contains(ViewModel.ModsFilter, StringComparison.InvariantCultureIgnoreCase);
        }
    }

    public class SetDependenciesDialogViewmodel : ObservableObject
    {
        public ObservableCollection<BooleanGenericTuple<IModConfig>> Dependencies { get; set; } = new ObservableCollection<BooleanGenericTuple<IModConfig>>();
        public ModConfigService ConfigService { get; }
        public PathTuple<ModConfig> CurrentMod { get; }
        public string ModsFilter { get; set; } = "";

        public SetDependenciesDialogViewmodel(ModConfigService configService, PathTuple<ModConfig> modConfig)
        {
            ConfigService = configService;
            CurrentMod = modConfig;

            PopulateDependencies();
        }

        public void SaveCurrentMod()
        {
            CurrentMod.Config.ModDependencies = Dependencies.Where(x => x.Enabled).Select(x => x.Generic.ModId).ToArray();
            CurrentMod.Save();
        }

        private void PopulateDependencies()
        {
            var mods = ConfigService.Items; // In case collection changes during window open.
            foreach (var mod in mods)
            {
                bool enabled = CurrentMod.Config.ModDependencies.Contains(mod.Config.ModId);
                Dependencies.Add(new BooleanGenericTuple<IModConfig>(enabled, mod.Config));
            }
        }
    }
}
