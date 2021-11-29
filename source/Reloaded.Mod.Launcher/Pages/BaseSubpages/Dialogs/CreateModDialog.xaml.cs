using System.Windows;
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
        private XamlResource<string> _xamlTitleCreateModNonUniqueId   = new XamlResource<string>("TitleCreateModNonUniqueId");
        private XamlResource<string> _xamlMessageCreateModNonUniqueId = new XamlResource<string>("MessageCreateModNonUniqueId");

        public CreateModViewModel RealViewModel { get; set; }

        public CreateModDialog(ModConfigService modConfigService)
        {
            InitializeComponent();
            RealViewModel = new CreateModViewModel(modConfigService);
        }

        public async void Save()
        {
            var mod = await RealViewModel.CreateMod(ShowNonUniqueWindow);
            if (mod == null)
                return;

            var createModDialog = new EditModDialog(mod, IoC.Get<ApplicationConfigService>(), IoC.Get<ModConfigService>());
            createModDialog.Owner = Window.GetWindow(this);
            createModDialog.ShowDialog();
            this.Close();
        }

        private void ShowNonUniqueWindow()
        {
            var messageBoxDialog = new MessageBox(_xamlTitleCreateModNonUniqueId.Get(), _xamlMessageCreateModNonUniqueId.Get());
            messageBoxDialog.Owner = this;
            messageBoxDialog.ShowDialog();
        }
    }
}
