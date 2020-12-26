using System.Linq;
using System.Windows;
using System.Windows.Input;
using Reloaded.Mod.Launcher.Models.ViewModel;
using Reloaded.Mod.Launcher.Models.ViewModel.Dialogs;
using Reloaded.Mod.Loader.IO.Services;
using Reloaded.WPF.Theme.Default;
using Reloaded.WPF.Utilities;
using MessageBox = Reloaded.Mod.Launcher.Pages.Dialogs.MessageBox;

namespace Reloaded.Mod.Launcher.Pages.BaseSubpages.Dialogs
{
    /// <summary>
    /// Interaction logic for CreateModDialog.xaml
    /// </summary>
    public partial class CreateModDialog : ReloadedWindow
    {
        public CreateModViewModel RealViewModel  { get; set; }
        public ModConfigService ModConfigService { get; set; }

        private XamlResource<string> _xamlTitleCreateModNonUniqueId = new XamlResource<string>("TitleCreateModNonUniqueId");
        private XamlResource<string> _xamlMessageCreateModNonUniqueId = new XamlResource<string>("MessageCreateModNonUniqueId");

        public CreateModDialog(ModConfigService service)
        {
            InitializeComponent();
            ModConfigService = service;
            RealViewModel = new CreateModViewModel(ModConfigService, new DictionaryResourceManipulator(this.Contents.Resources));
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

            if (!IsUnique()) 
                return;

            RealViewModel.Save();
            var window = Window.GetWindow((DependencyObject)sender);
            window.Close();
        }

        /* Check if not duplicate. */
        public bool IsUnique()
        {
            if (ModConfigService.Mods.Any(x => x.Config.ModId.Equals(RealViewModel.Config.ModId)))
            {
                var messageBoxDialog = new MessageBox(_xamlTitleCreateModNonUniqueId.Get(),
                                                      _xamlMessageCreateModNonUniqueId.Get());
                messageBoxDialog.Owner = this;
                messageBoxDialog.ShowDialog();
                return false;
            }

            return true;
        }

        private void ModsFilter_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e) => RealViewModel.RefreshModList();
    }
}
