using System.Windows;
using System.Windows.Input;
using Reloaded.WPF.Theme.Default;

namespace Reloaded.Mod.Launcher.Pages.Dialogs
{
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

        private void OK_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DialogResult = true;
                this.Close();
            }
        }

        private void Cancel_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DialogResult = false;
                this.Close();
            }
        }
    }
}
