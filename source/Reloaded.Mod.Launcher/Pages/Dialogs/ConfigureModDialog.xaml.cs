namespace Reloaded.Mod.Launcher.Pages.Dialogs;

/// <summary>
/// Interaction logic for ConfigureModDialog.xaml
/// </summary>
public partial class ConfigureModDialog : ReloadedWindow
{
    public new ConfigureModDialogViewModel ViewModel { get; set; }

    public ConfigureModDialog(ConfigureModDialogViewModel viewModel)
    {
        ViewModel = viewModel;
        InitializeComponent();
    }

    private void Save_Click(object sender, RoutedEventArgs e) => ViewModel.Save();

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) => ViewModel.Save();

    private void Reset_Click(object sender, RoutedEventArgs e)
    {
        // TODO: Reset should probably be in ViewModel and platform-agnostic.
        PropertyGridEx.ResetValues();
    }
}