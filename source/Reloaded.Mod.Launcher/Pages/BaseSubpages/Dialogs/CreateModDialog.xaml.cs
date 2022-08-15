using MessageBox = Reloaded.Mod.Launcher.Pages.Dialogs.MessageBox;
using Window = System.Windows.Window;

namespace Reloaded.Mod.Launcher.Pages.BaseSubpages.Dialogs;

/// <summary>
/// Interaction logic for CreateModDialog.xaml
/// </summary>
public partial class CreateModDialog : ReloadedWindow
{
    public CreateModViewModel RealViewModel { get; set; }

    public CreateModDialog(ModConfigService modConfigService)
    {
        InitializeComponent();
        RealViewModel = new CreateModViewModel(modConfigService);
    }

    public async Task Save()
    {
        var createdMod = await RealViewModel.CreateMod(ShowNonUniqueWindow);
        if (createdMod == null)
            return;

        var modConfigService = Lib.IoC.Get<ModConfigService>();
        var mod = await ActionWrappers.TryGetValueAsync(() => modConfigService.ItemsById[createdMod.Config.ModId], 5000, 32);
        if (mod != null)
        {
            var createModDialog = new EditModDialog(new EditModDialogViewModel(mod, Lib.IoC.Get<ApplicationConfigService>(), modConfigService));
            createModDialog.Owner = Window.GetWindow(this);
            createModDialog.ShowDialog();
        }

        this.Close();
    }

    private void ShowNonUniqueWindow()
    {
        var messageBoxDialog = new MessageBox(Lib.Static.Resources.TitleCreateModNonUniqueId.Get(), Lib.Static.Resources.MessageCreateModNonUniqueId.Get());
        messageBoxDialog.Owner = this;
        messageBoxDialog.ShowDialog();
    }

    private async void Save_Click(object sender, RoutedEventArgs e) => await Save();
}