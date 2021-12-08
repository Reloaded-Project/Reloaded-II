using System.Windows;
using Reloaded.Mod.Launcher.Lib.Models.ViewModel.Dialog;
using Reloaded.WPF.Theme.Default;

namespace Reloaded.Mod.Launcher.Pages.Dialogs;

/// <summary>
/// Interaction logic for FirstLaunch.xaml
/// </summary>
public partial class ModUpdateDialog : ReloadedWindow
{
    public new ModUpdateDialogViewModel ViewModel { get; set; }

    public ModUpdateDialog(ModUpdateDialogViewModel viewModel)
    {
        ViewModel = viewModel;
        InitializeComponent();
    }

    private async void Button_Click(object sender, RoutedEventArgs e)
    {
        if (await ViewModel.Update())
            this.Close();
    }
}