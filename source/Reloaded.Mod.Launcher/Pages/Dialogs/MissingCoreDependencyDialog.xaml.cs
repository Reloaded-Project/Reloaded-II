namespace Reloaded.Mod.Launcher.Pages.Dialogs;

/// <summary>
/// Interaction logic for MissingCoreDependencyDialog.xaml
/// </summary>
public partial class MissingCoreDependencyDialog : ReloadedWindow
{
    public new MissingCoreDependencyDialogViewModel ViewModel { get; set; }

    public MissingCoreDependencyDialog(MissingCoreDependencyDialogViewModel viewModel)
    {
        ViewModel = viewModel;
        InitializeComponent();
    }

    private async void DownloadButtonClick(object sender, RoutedEventArgs e)
    {
        if (await ViewModel.DownloadAndInstallMissingDependenciesAsync())
            this.Close();
    }
}