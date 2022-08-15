namespace Reloaded.Mod.Launcher.Pages.Dialogs.FirstLaunchPages;

/// <summary>
/// Interaction logic for Complete.xaml
/// </summary>
public partial class Complete : ReloadedPage
{
    public CompleteViewModel ViewModel { get; set; } = Lib.IoC.Get<CompleteViewModel>();

    public Complete()
    {
        InitializeComponent();
    }

    private void OK_Click(object sender, RoutedEventArgs e) => ViewModel.Close();

    private void Documents_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        ViewModel.OpenDocumentation();
    }

    private void UserGuide_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        ViewModel.OpenUserGuide();
    }

    private void Previous_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.GoToPreviousPage();
    }
}