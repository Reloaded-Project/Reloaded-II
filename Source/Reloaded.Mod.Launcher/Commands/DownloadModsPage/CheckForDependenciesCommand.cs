using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Reloaded.Mod.Launcher.Commands.Templates;
using Reloaded.Mod.Launcher.Models.ViewModel;
using Reloaded.WPF.Utilities;
using MessageBox = Reloaded.Mod.Launcher.Pages.Dialogs.MessageBox;

namespace Reloaded.Mod.Launcher.Commands.DownloadModsPage
{
    public class CheckForDependenciesCommand : WithCanExecuteChanged, ICommand
    {
        private XamlResource<string> _noUpdateDialogTitle   = new XamlResource<string>("NoUpdateDialogTitle");
        private XamlResource<string> _noUpdateDialogMessage = new XamlResource<string>("NoUpdateDialogMessage");
        private ManageModsViewModel _manageModsViewModel;
        private bool _canExecute = true;

        public CheckForDependenciesCommand()
        {
            _manageModsViewModel = IoC.Get<ManageModsViewModel>();
        }

        /* Interface */
        public bool CanExecute(object parameter)
        {
            return _canExecute;
        }

        public async void Execute(object parameter)
        {
            _canExecute = false;
            RaiseCanExecute(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));

            if (Update.CheckMissingDependencies(out var missingDependencies))
            {
                await Update.DownloadPackagesAsync(missingDependencies, true, true);
            }
            else
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
