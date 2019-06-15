using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Input;
using Reloaded.Mod.Launcher.Commands.Templates;
using Reloaded.Mod.Launcher.Models.ViewModel;
using Reloaded.Mod.Launcher.Pages.BaseSubpages.Dialogs;

namespace Reloaded.Mod.Launcher.Commands.ManageModsPage
{
    public class ManageDependenciesCommand : WithCanExecuteChanged, ICommand, IDisposable
    {
        private ManageModsViewModel _manageModsViewModel;

        public ManageDependenciesCommand()
        {
            _manageModsViewModel = IoC.Get<ManageModsViewModel>();
            _manageModsViewModel.PropertyChanged += ManageModsViewModelPropertyChanged;
        }

        ~ManageDependenciesCommand()
        {
            Dispose();
        }

        public void Dispose()
        {
            _manageModsViewModel.PropertyChanged -= ManageModsViewModelPropertyChanged;
            GC.SuppressFinalize(this);
        }

        /* Implementation */

        private void ManageModsViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(_manageModsViewModel.SelectedModPathTuple))
                RaiseCanExecute(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        /* ICommand. */

        public bool CanExecute(object parameter)
        {
            if (_manageModsViewModel.SelectedModPathTuple == null)
                return false;

            return true;
        }

        public void Execute(object parameter)
        {
            _manageModsViewModel.InvokeWithoutMonitoringMods(() =>
            {
                var dialog = new SetDependenciesDialog(_manageModsViewModel);
                dialog.ShowDialog();
            });
        }
    }
}
