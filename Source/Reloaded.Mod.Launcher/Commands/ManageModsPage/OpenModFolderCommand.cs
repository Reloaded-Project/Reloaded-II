using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Reloaded.Mod.Launcher.Commands.Templates;
using Reloaded.Mod.Launcher.Models.ViewModel;

namespace Reloaded.Mod.Launcher.Commands.ManageModsPage
{
    public class OpenModFolderCommand : WithCanExecuteChanged, ICommand, IDisposable
    {
        private ManageModsViewModel _manageModsViewModel;

        public OpenModFolderCommand()
        {
            _manageModsViewModel = IoC.Get<ManageModsViewModel>();
            _manageModsViewModel.PropertyChanged += ManageModsViewModelPropertyChanged;
        }

        ~OpenModFolderCommand()
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

        /* ICommand */

        public bool CanExecute(object parameter)
        {
            if (_manageModsViewModel.SelectedModPathTuple != null)
            {
                string directoryPath = Path.GetDirectoryName(_manageModsViewModel.SelectedModPathTuple.ModConfigPath);
                if (Directory.Exists(directoryPath))
                    return true;
            }

            return false;
        }

        [SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")]
        public void Execute(object parameter)
        {
            string directoryPath = Path.GetDirectoryName(_manageModsViewModel.SelectedModPathTuple.ModConfigPath);
            Process.Start(directoryPath);
        }
    }
}
