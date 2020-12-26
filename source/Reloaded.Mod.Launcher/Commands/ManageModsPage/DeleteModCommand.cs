using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using Reloaded.Mod.Launcher.Commands.Templates;
using Reloaded.Mod.Launcher.Misc;
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
            if (e.PropertyName == nameof(_manageModsViewModel.SelectedModTuple))
                RaiseCanExecute(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }


        /* ICommand. */

        public bool CanExecute(object parameter)
        {
            if (_manageModsViewModel.SelectedModTuple == null)
                return false;

            return true;
        }

        public void Execute(object parameter)
        {
            // Find mod in mod list.
            var app   = _manageModsViewModel.SelectedModTuple;
            var entry = _manageModsViewModel.ModConfigService.Mods.First(x => x.Config.Equals(app.Config));

            // Delete folder contents.
            var directory = Path.GetDirectoryName(entry.Path) ?? throw new InvalidOperationException(Errors.FailedToGetDirectoryOfMod());
            Directory.Delete(directory, true);
        }
    }
}
