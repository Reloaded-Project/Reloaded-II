using System.Windows;
using Reloaded.Mod.Launcher.Commands.Dialog;
using Reloaded.WPF.Theme.Default;

namespace Reloaded.Mod.Launcher.Pages.Dialogs
{
    /// <summary>
    /// Interaction logic for FirstLaunch.xaml
    /// </summary>
    public partial class FirstLaunch : ReloadedWindow
    {
        public FirstLaunch()
        {
            InitializeComponent();
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Documents_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            new OpenDocumentationCommand().Execute(null);
        }

        private void UserGuide_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            new OpenUserGuideCommand().Execute(null);
        }
    }
}
