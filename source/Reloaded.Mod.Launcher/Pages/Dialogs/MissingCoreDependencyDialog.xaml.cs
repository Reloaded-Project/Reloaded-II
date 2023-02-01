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
        var model = (MissingDependency)((FrameworkElement)sender).DataContext;
        await model.OpenUrlsAsync();
    }
}