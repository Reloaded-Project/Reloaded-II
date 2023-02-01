using Button = System.Windows.Controls.Button;

namespace Reloaded.Mod.Launcher.Pages.Dialogs;

/// <summary>
/// Interaction logic for DownloadPackageDialog.xaml
/// </summary>
public partial class DownloadPackageDialog : ReloadedWindow
{
    public new DownloadPackageViewModel ViewModel { get; set; }

    /// <inheritdoc />
    public DownloadPackageDialog(DownloadPackageViewModel viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel;

        // Close on completion if task already started.
        if (ViewModel.DownloadTask != null)
        {
            ViewModel.DownloadTask.ContinueWith(task =>
            {
                ActionWrappers.ExecuteWithApplicationDispatcher(this.Close);
            });

            Button.IsEnabled = false;
        }

        Closed += OnClosed;
    }

    private void OnClosed(object? sender, EventArgs e) => ViewModel.DownloadToken.Cancel();

    private async void Download_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        var senderButton = (Button)sender;
        senderButton.IsEnabled = false; // Breaking MVVM, don't care in this case.
        await ViewModel.StartDownloadAsync();
        this.Close();
    }
}