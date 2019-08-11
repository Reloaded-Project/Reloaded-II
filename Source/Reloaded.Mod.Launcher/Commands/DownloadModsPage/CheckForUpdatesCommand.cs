using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Reloaded.Mod.Launcher.Commands.Templates;
using Reloaded.WPF.Utilities;
using MessageBox = Reloaded.Mod.Launcher.Pages.Dialogs.MessageBox;

namespace Reloaded.Mod.Launcher.Commands.DownloadModsPage
{
    public class CheckForUpdatesCommand : WithCanExecuteChanged, ICommand
    {
        private XamlResource<string> _noUpdateDialogTitle   = new XamlResource<string>("NoUpdateDialogTitle");
        private XamlResource<string> _noUpdateDialogMessage = new XamlResource<string>("NoUpdateDialogMessage");
        private bool _canExecute = true;

        /* ICommand. */

        public bool CanExecute(object parameter)
        {
            return _canExecute;
        }

        public async void Execute(object parameter)
        {
            _canExecute = false;
            RaiseCanExecute(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));

            var updates = await Task.Run(Update.CheckForModUpdatesAsync);

            if (! updates)
            {
                var box = new MessageBox(_noUpdateDialogTitle.Get(),
                                         _noUpdateDialogMessage.Get());
                box.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                box.ShowDialog();
            }

            _canExecute = true;
            RaiseCanExecute(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }
}
