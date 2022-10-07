using System.Windows.Navigation;

namespace Reloaded.Mod.Launcher.Pages.Dialogs;

/// <summary>
/// Interaction logic for FirstLaunch.xaml
/// </summary>
public partial class ModUpdateDialog : ReloadedWindow
{
    public new ModUpdateDialogViewModel ViewModel { get; set; }

    public ModUpdateDialog(ModUpdateDialogViewModel viewModel)
    {
        ViewModel = viewModel;
        InitializeComponent();
    }

    private async void Button_Click(object sender, RoutedEventArgs e)
    {
        if (await ViewModel.Update())
            this.Close();
    }

    private void OpenHyperlink(object sender, ExecutedRoutedEventArgs e) => ThemeHelpers.OpenHyperlink(sender, e);

    // Don't navigate hyperlinks in our markdown, thanks!
    // Not sure if this is needed on non Page items, but just in case.
    private void RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e) => e.Handled = true;
}