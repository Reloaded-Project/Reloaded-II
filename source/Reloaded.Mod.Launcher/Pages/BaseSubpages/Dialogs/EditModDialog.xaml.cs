using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Reloaded.Mod.Launcher.Models.ViewModel.Dialogs;
using Reloaded.Mod.Loader.IO.Config;
using Reloaded.Mod.Loader.IO.Services;
using Reloaded.Mod.Loader.IO.Structs;
using Reloaded.WPF.Theme.Default;
using Reloaded.WPF.Utilities;
using MessageBox = Reloaded.Mod.Launcher.Pages.Dialogs.MessageBox;

namespace Reloaded.Mod.Launcher.Pages.BaseSubpages.Dialogs
{
    /// <summary>
    /// Interaction logic for CreateModDialog.xaml
    /// </summary>
    public partial class EditModDialog : ReloadedWindow
    {
        private readonly ApplicationConfigService _appConfigService;

        public EditModViewModel RealViewModel  { get; set; }

        public ModConfigService ModConfigService { get; set; }

        public EditModDialog(PathTuple<ModConfig> modConfig, ApplicationConfigService appConfigService, ModConfigService modConfigService)
        {
            InitializeComponent();
            _appConfigService = appConfigService;
            ModConfigService  = modConfigService;
            RealViewModel = new EditModViewModel(modConfig, ModConfigService, new DictionaryResourceManipulator(this.Contents.Resources));
            this.Closing += OnClosing;
        }

        private void OnClosing(object sender, CancelEventArgs e)
        {
            RealViewModel.Save();
        }

        private void ModIcon_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed) 
                return;

            this.RealViewModel.Image = RealViewModel.GetImage();
        }

        private void Save_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed) 
                return;
            
            this.Close();
        }

        /* Check if not duplicate. */

        private void ModsFilter_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e) => RealViewModel.RefreshModList();
    }
}
