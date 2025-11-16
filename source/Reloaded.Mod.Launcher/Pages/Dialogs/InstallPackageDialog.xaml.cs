using Button = System.Windows.Controls.Button;

namespace Reloaded.Mod.Launcher.Pages.Dialogs;

/// <summary>
/// Interaction logic for DownloadPackageDialog.xaml
/// </summary>
public partial class InstallPackageDialog : ReloadedWindow
{
    public new InstallPackageViewModel ViewModel { get; set; }

    /// <inheritdoc />
    public InstallPackageDialog(InstallPackageViewModel viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel;
        DataContext = viewModel;
        viewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(InstallPackageViewModel.IsComplete) && viewModel.IsComplete)
            {
                ActionWrappers.ExecuteWithApplicationDispatcher(this.Close);
            }
        };
    }
}