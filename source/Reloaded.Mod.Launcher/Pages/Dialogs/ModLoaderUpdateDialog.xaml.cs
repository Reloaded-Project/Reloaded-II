using WindowViewModel = Reloaded.WPF.Theme.Default.WindowViewModel;

namespace Reloaded.Mod.Launcher.Pages.Dialogs;

/// <summary>
/// Interaction logic for FirstLaunch.xaml
/// </summary>
public partial class ModLoaderUpdateDialog : ReloadedWindow
{
    public new ModLoaderUpdateDialogViewModel ViewModel { get; set; }
        
    public ModLoaderUpdateDialog(ModLoaderUpdateDialogViewModel viewmodel)
    {
        ViewModel = viewmodel;
        InitializeComponent();
        var model = (WindowViewModel) this.DataContext;
        model.MinimizeButtonVisibility = Visibility.Collapsed;
        model.MaximizeButtonVisibility = Visibility.Collapsed;
    }

    private void ViewChangelogClick(object sender, RoutedEventArgs e) => ViewModel.ViewChangelog();

    private async void UpdateClick(object sender, RoutedEventArgs e) => await ViewModel.Update();

    private void OpenHyperlink(object sender, System.Windows.Input.ExecutedRoutedEventArgs e) => ProcessExtensions.OpenFileWithDefaultProgram(e.Parameter.ToString()!);
}