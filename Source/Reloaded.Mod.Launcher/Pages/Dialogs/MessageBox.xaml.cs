using System.Windows;
using System.Windows.Input;
using Reloaded.WPF.Theme.Default;

namespace Reloaded.Mod.Launcher.Pages.Dialogs
{
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

        private void Button_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.Close();
            }
        }
    }
}
