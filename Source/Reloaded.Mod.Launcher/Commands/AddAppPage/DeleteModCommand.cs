using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Reloaded.Mod.Launcher.Models.ViewModel;

namespace Reloaded.Mod.Launcher.Commands.AddAppPage
{
    /// <summary>
    /// Comnmand to be used by the <see cref="ManageModsPage"/> which decides
    /// whether the current entry can be removed.
    /// </summary>
    public class DeleteModCommand : ICommand, IDisposable
    {
        private ManageModsViewModel _manageModsViewModel;

        public DeleteModCommand()
        {
            _manageModsViewModel = IoC.Get<ManageModsViewModel>();
            _manageModsViewModel.PropertyChanged += ManageModsViewModelPropertyChanged;
        }

        ~DeleteModCommand()
        {
            Cleanup();
        }

        public void Dispose()
        {
            Cleanup();
            GC.SuppressFinalize(this);
        }

        private void Cleanup()
        {
            _manageModsViewModel.PropertyChanged -= ManageModsViewModelPropertyChanged;
        }

        /* Implementation */

        private void ManageModsViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(_manageModsViewModel.SelectedModPathTuple))
                RaiseCanExecute(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        /* Helper(s) */

        private void RaiseCanExecute(object sender, NotifyCollectionChangedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke( () => { CanExecuteChanged(sender, e); } );
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
            // Find Application in Viewmodel's list and remove it.
            var app = _manageModsViewModel.SelectedModPathTuple;
            var entry = _manageModsViewModel.Mods.First(x => x.ModConfig.Equals(app.ModConfig));
            _manageModsViewModel.Mods.Remove(entry);

            // Delete folder contents.
            Directory.Delete(Path.GetDirectoryName(entry.ModConfigPath), true);

            // File system watcher automatically updates collection in MainPageViewModel.Applications
        }

        public event EventHandler CanExecuteChanged = (sender, args) => { };
    }
}
