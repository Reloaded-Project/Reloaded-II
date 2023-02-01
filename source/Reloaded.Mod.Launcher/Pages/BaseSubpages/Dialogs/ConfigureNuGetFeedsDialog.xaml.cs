namespace Reloaded.Mod.Launcher.Pages.BaseSubpages.Dialogs;

/// <summary>
/// Interaction logic for ConfigureNuGetFeedsDialog.xaml
/// </summary>
public partial class ConfigureNuGetFeedsDialog : ReloadedWindow, IDisposable
{
    public ConfigureNuGetFeedsDialogViewModel RealViewModel { get; set; }

    public ConfigureNuGetFeedsDialog(ConfigureNuGetFeedsDialogViewModel viewModel)
    {
        InitializeComponent();
        RealViewModel = viewModel;
        this.Closed += SaveOnClose;
    }

    /// <inheritdoc />
    public void Dispose() => RealViewModel?.Dispose();

    private void SaveOnClose(object? sender, EventArgs e)
    {
        RealViewModel.Save();
        Dispose();
    }
}