namespace Reloaded.Mod.Launcher.Pages.Dialogs;

/// <summary>
/// Interaction logic for SelectAddedGameDialog.xaml
/// </summary>
public partial class SelectAddedGameDialog : ReloadedWindow
{
    /// <summary>
    /// Exposes the viewmodel of the dialog.
    /// </summary>
    public new SelectAddedGameDialogViewModel ViewModel { get; set; }

    public SelectAddedGameDialog(SelectAddedGameDialogViewModel viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        this.Close();
    }

    private void OK_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        this.Close();
    }
}