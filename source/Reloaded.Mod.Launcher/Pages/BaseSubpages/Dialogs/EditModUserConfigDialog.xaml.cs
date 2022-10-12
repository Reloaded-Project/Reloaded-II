namespace Reloaded.Mod.Launcher.Pages.BaseSubpages.Dialogs;

/// <summary>
/// Interaction logic for EditModUserConfigDialog.xaml
/// </summary>
public partial class EditModUserConfigDialog : ReloadedWindow
{
    public EditModUserConfigDialogViewModel RealViewModel { get; set; }

    public EditModUserConfigDialog(EditModUserConfigDialogViewModel realViewModel)
    {
        InitializeComponent();
        RealViewModel = realViewModel;
        this.Closing += OnClosing;
    }

    private async void OnClosing(object? sender, CancelEventArgs e) => await RealViewModel.SaveAsync();
}