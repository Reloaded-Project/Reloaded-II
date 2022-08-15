using WindowViewModel = Reloaded.WPF.Theme.Default.WindowViewModel;

namespace Reloaded.Mod.Launcher.Pages.Dialogs;

/// <summary>
/// Interaction logic for MessageBoxOkCancel.xaml
/// </summary>
public partial class MessageBoxOkCancel : ReloadedWindow
{
    public MessageBoxOkCancel(string title, string message)
    {
        InitializeComponent();
        this.Title = title;
        this.Message.Text = message;
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
}