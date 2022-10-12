namespace Reloaded.Mod.Launcher.Pages.BaseSubpages.Dialogs;

/// <summary>
/// Interaction logic for CreateModDialog.xaml
/// </summary>
public partial class EditModDialog : ReloadedWindow
{
    public EditModDialogViewModel RealViewModel  { get; set; }

    public EditModDialog(EditModDialogViewModel viewModel)
    {
        InitializeComponent();
        RealViewModel = viewModel;
        RealViewModel.Init(this.Close);

        this.Closing += OnClosing;
    }

    private void OnClosing(object? sender, CancelEventArgs e)
    {
        RealViewModel.Save();
        RealViewModel.Dispose(); // Unbind Constant.
    }

    private void Last_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        RealViewModel.Page -= 1;
    }

    private void Next_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        RealViewModel.Page += 1;
    }

    private void Save_Click(object sender, System.Windows.Input.MouseButtonEventArgs e) => Close();
}