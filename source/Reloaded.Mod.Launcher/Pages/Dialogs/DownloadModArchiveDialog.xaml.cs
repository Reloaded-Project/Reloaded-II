using System.Windows;
using System.Windows.Controls;
using Reloaded.Mod.Launcher.Lib.Models.ViewModel.Dialog;
using Reloaded.WPF.Theme.Default;

namespace Reloaded.Mod.Launcher.Pages.Dialogs;

/// <summary>
/// Interaction logic for DownloadModArchive.xaml
/// </summary>
public partial class DownloadModArchiveDialog : ReloadedWindow
{
    public new DownloadModArchiveViewModel ViewModel { get; set; }

    public DownloadModArchiveDialog(DownloadModArchiveViewModel viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel;
    }

    private async void Download_Click(object sender, RoutedEventArgs e)
    {
        var senderButton = (Button) sender;
        senderButton.IsEnabled = false; // Breaking MVVM, don't care in this case.
        await ViewModel.DownloadAndExtractAll();
        this.Close();
    }
}