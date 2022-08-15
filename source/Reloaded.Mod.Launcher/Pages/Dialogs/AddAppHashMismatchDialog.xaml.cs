namespace Reloaded.Mod.Launcher.Pages.Dialogs;

/// <summary>
/// Interaction logic for AddAppHashMismatchDialog.xaml
/// </summary>
public partial class AddAppHashMismatchDialog : ReloadedWindow
{
    public new AddAppHashMismatchDialogViewModel ViewModel { get; set; }

    public AddAppHashMismatchDialog(AddAppHashMismatchDialogViewModel viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel;
    }

    private void OK_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed)
        {
            DialogResult = true;
            this.Close();
        }
    }
}