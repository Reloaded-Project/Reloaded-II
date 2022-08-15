using WindowViewModel = Reloaded.WPF.Theme.Default.WindowViewModel;

namespace Reloaded.Mod.Launcher.Pages.Dialogs;

/// <summary>
/// Interaction logic for MessageBox.xaml
/// </summary>
public partial class MessageBox : ReloadedWindow
{
    public MessageBox(string title, string message) : base()
    {
        InitializeComponent();
        this.Title = title;
        this.Message.Text = message;
        var viewModel = ((WindowViewModel)this.DataContext);

        viewModel.MinimizeButtonVisibility = Visibility.Collapsed;
        viewModel.MaximizeButtonVisibility = Visibility.Collapsed;
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        this.DialogResult = true;
        this.Close();
    }
}