using System;
using System.Linq;
using System.Windows;
using Reloaded.Mod.Launcher.Utility;
using Reloaded.Mod.Loader.IO.Config;
using Reloaded.Mod.Loader.Update;
using Reloaded.Mod.Loader.Update.Structures;
using Reloaded.WPF.MVVM;
using Reloaded.WPF.Theme.Default;
using Reloaded.WPF.Utilities;

namespace Reloaded.Mod.Launcher.Pages.Dialogs
{
    /// <summary>
    /// Interaction logic for FirstLaunch.xaml
    /// </summary>
    public partial class ModUpdateDialog : ReloadedWindow
    {
        private static XamlResource<string> _xamlUpdateModConfirmTitle = new XamlResource<string>("UpdateModConfirmTitle");
        private static XamlResource<string> _xamlUpdateModConfirmMessage = new XamlResource<string>("UpdateModConfirmMessage");

        public new ModUpdateDialogViewModel ViewModel { get; set; }

        public ModUpdateDialog(Updater updater, ModUpdateSummary summary)
        {
            InitializeComponent();
            ViewModel = new ModUpdateDialogViewModel(updater, summary);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                void Update()
                {
                    ViewModel.Updater
                        .Update(ViewModel.Summary, new Progress<double>(d => { ViewModel.Progress = (int)(d * 100); }))
                        .ContinueWith(x => this.Dispatcher.Invoke(this.Close));
                }
                
                if (ApplicationInstanceTracker.GetAllProcesses(out _))
                {
                    var messageBox = new MessageBoxOkCancel(_xamlUpdateModConfirmTitle.Get(), _xamlUpdateModConfirmMessage.Get());
                    messageBox.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                    messageBox.ShowDialog();

                    if (messageBox.DialogResult.HasValue && messageBox.DialogResult.Value)
                        Update();
                }
                else
                {
                    Update();   
                }
            });
        }
    }

    public class ModUpdateDialogViewModel : ObservableObject
    {
        public Updater      Updater         { get; set; }
        public ModUpdateSummary Summary     { get; set; }
        public ModUpdate[]  UpdateInfo      { get; set; }
        public long         TotalSize       { get; set; }

        public int          Progress         { get; set; }

        public ModUpdateDialogViewModel(Updater updater, ModUpdateSummary summary)
        {
            Updater = updater;
            Summary = summary;
            UpdateInfo = Summary.GetUpdateInfo();
            TotalSize = Summary.GetTotalSize();
        }
    }
}
