namespace Reloaded.Mod.Launcher.Pages.Dialogs.EditModPages;

/// <summary>
/// Interaction logic for Updates.xaml
/// </summary>
public partial class Updates : ReloadedPage
{
    public EditModDialogViewModel ViewModel { get; set; }

    public Updates(EditModDialogViewModel viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel;
    }
}