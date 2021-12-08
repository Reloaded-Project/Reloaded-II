using System.Windows;
using Reloaded.Mod.Launcher.Lib.Models.ViewModel.Dialog;
using Reloaded.WPF.Theme.Default;

namespace Reloaded.Mod.Launcher.Pages.Dialogs;

/// <summary>
/// Interaction logic for NugetFetchPackageDialog.xaml
/// </summary>
public partial class NugetFetchPackageDialog : ReloadedWindow
{
    public new NugetFetchPackageDialogViewModel ViewModel { get; set; }

    public NugetFetchPackageDialog(NugetFetchPackageDialogViewModel viewModel)
    {
        ViewModel = viewModel;
        InitializeComponent();
    }

    private async void OK_Click(object sender, RoutedEventArgs e)
    {
        await ViewModel.DownloadAndExtractPackagesAsync();
        this.Close();
    }
}