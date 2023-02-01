using WindowViewModel = Reloaded.WPF.Theme.Default.WindowViewModel;

namespace Reloaded.Mod.Launcher.Pages.Dialogs;

/// <summary>
/// Interaction logic for MessageBoxOkCancel.xaml
/// </summary>
public partial class RunAppViaWineDialog : ReloadedWindow
{
    public RunAppViaWineDialog()
    {
        InitializeComponent();
        var viewModel = ((WindowViewModel)this.DataContext);
        viewModel.MinimizeButtonVisibility = Visibility.Collapsed;
        viewModel.MaximizeButtonVisibility = Visibility.Collapsed;
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

    private void OpenHyperlink(object sender, System.Windows.Navigation.RequestNavigateEventArgs e) => ProcessExtensions.OpenFileWithDefaultProgram(e.Uri.ToString());
}