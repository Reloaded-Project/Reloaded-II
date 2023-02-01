namespace Reloaded.Mod.Launcher.Pages.Dialogs.EditModPages;

/// <summary>
/// Interaction logic for Main.xaml
/// </summary>
public partial class Main : ReloadedPage
{
    public EditModDialogViewModel ViewModel { get; set; }

    public Main(EditModDialogViewModel viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel;
    }

    private void ModIcon_PreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        ViewModel.SetNewImage();
    }

    private void AddTag_Click(object sender, RoutedEventArgs e) => ViewModel.AddCurrentTag();
}