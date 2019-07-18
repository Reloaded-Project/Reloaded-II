using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using Reloaded.Mod.Launcher.Commands.Templates;
using Reloaded.Mod.Launcher.Models.ViewModel;

namespace Reloaded.Mod.Launcher.Commands.ManageModsPage
{
    public class DeleteModCommand : WithCanExecuteChanged, ICommand, IDisposable
    {
        private readonly ManageModsViewModel _manageModsViewModel;

        public DeleteModCommand()
        {
            _manageModsViewModel = IoC.Get<ManageModsViewModel>();
            _manageModsViewModel.PropertyChanged += ManageModsViewModelPropertyChanged;
        }

        ~DeleteModCommand()
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
            // Find Application in Viewmodel's list and remove it.
            var app = _manageModsViewModel.SelectedModPathTuple;
            var entry = _manageModsViewModel.Mods.First(x => x.ModConfig.Equals(app.ModConfig));
            _manageModsViewModel.Mods.Remove(entry);

            // Delete folder contents.
            var directory = Path.GetDirectoryName(entry.ModConfigPath) ?? throw new InvalidOperationException(Errors.FailedToGetDirectoryOfMod());
            Directory.Delete(directory, true);

            // File system watcher automatically updates collection in MainPageViewModel.Applications
        }
    }
}
