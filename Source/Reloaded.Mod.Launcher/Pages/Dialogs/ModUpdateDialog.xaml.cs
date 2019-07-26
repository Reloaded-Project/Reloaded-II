using System;
using System.Collections.ObjectModel;
using System.Windows;
using Reloaded.Mod.Launcher.Commands.Dialog;
using Reloaded.Mod.Loader.Update;
using Reloaded.Mod.Loader.Update.Structures;
using Reloaded.WPF.MVVM;
using Reloaded.WPF.Theme.Default;

namespace Reloaded.Mod.Launcher.Pages.Dialogs
{
    /// <summary>
    /// Interaction logic for FirstLaunch.xaml
    /// </summary>
    public partial class ModUpdateDialog : ReloadedWindow
    {
        public ModUpdateDialogViewModel ViewModel { get; set; }

        public ModUpdateDialog(Updater updater, ModUpdateSummary summary)
        {
            InitializeComponent();
            ViewModel = new ModUpdateDialogViewModel(updater, summary);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var updateTask = ViewModel.Updater.Update(ViewModel.Summary, new Progress<double>(d =>
            {
                ViewModel.Progress = (int)(d * 100);
            })).ContinueWith(x => this.Dispatcher.Invoke(this.Close));
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
