using System.Linq;
using System.Windows;
using System.Windows.Input;
using Reloaded.Mod.Launcher.Models.ViewModel;
using Reloaded.Mod.Launcher.Models.ViewModel.Dialogs;
using Reloaded.Mod.Launcher.Utility;
using Reloaded.WPF.Theme.Default;
using MessageBox = Reloaded.Mod.Launcher.Pages.Dialogs.MessageBox;

namespace Reloaded.Mod.Launcher.Pages.BaseSubpages.Dialogs
{
    /// <summary>
    /// Interaction logic for CreateModDialog.xaml
    /// </summary>
    public partial class CreateModDialog : ReloadedWindow
    {
        #region XAML Strings
        // ReSharper disable InconsistentNaming
        private const string XAML_TitleCreateModNonUniqueId = "TitleCreateModNonUniqueId";
        private const string XAML_MessageCreateModNonUniqueId = "MessageCreateModNonUniqueId";
        // ReSharper restore InconsistentNaming
        #endregion

        public CreateModViewModel RealViewModel { get; set; }

        public CreateModDialog()
        {
            InitializeComponent();
            RealViewModel = IoC.Get<CreateModViewModel>();
        }

        private void ModIcon_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.RealViewModel.Image = RealViewModel.GetImage();
            }
        }

        private void Save_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (IsUnique())
                {
                    RealViewModel.Save();
                    var window = Window.GetWindow((DependencyObject)sender);
                    window.Close();
                }
            }
        }

        /* Check if not duplicate. */
        public bool IsUnique()
        {
            var modsViewModel = IoC.Get<ManageModsViewModel>();
            if (modsViewModel.Mods.Count(x => x.ModConfig.ModId.Equals(RealViewModel.Config.ModId)) > 0)
            {
                var messageBoxDialog = new MessageBox(  ApplicationResourceAcquirer.GetTypeOrDefault<string>(XAML_TitleCreateModNonUniqueId),
                                                        ApplicationResourceAcquirer.GetTypeOrDefault<string>(XAML_MessageCreateModNonUniqueId));
                messageBoxDialog.Owner = this;
                messageBoxDialog.ShowDialog();
                return false;
            }

            return true;
        }
    }
}
