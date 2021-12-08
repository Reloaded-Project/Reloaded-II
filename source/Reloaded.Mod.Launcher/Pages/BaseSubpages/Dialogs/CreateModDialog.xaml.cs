using System.Windows;
using Reloaded.Mod.Launcher.Lib;
using Reloaded.Mod.Launcher.Lib.Models.ViewModel.Dialog;
using Reloaded.Mod.Loader.IO.Services;
using Reloaded.WPF.Theme.Default;
using MessageBox = Reloaded.Mod.Launcher.Pages.Dialogs.MessageBox;

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

    public async void Save()
    {
        var mod = await RealViewModel.CreateMod(ShowNonUniqueWindow);
        if (mod == null)
            return;

        var createModDialog = new EditModDialog(new EditModDialogViewModel(mod, IoC.Get<ApplicationConfigService>(), IoC.Get<ModConfigService>()));
        createModDialog.Owner = Window.GetWindow(this);
        createModDialog.ShowDialog();
        this.Close();
    }

    private void ShowNonUniqueWindow()
    {
        var messageBoxDialog = new MessageBox(Lib.Static.Resources.TitleCreateModNonUniqueId.Get(), Lib.Static.Resources.MessageCreateModNonUniqueId.Get());
        messageBoxDialog.Owner = this;
        messageBoxDialog.ShowDialog();
    }
}