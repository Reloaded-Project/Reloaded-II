using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Windows.Input;
using Reloaded.Mod.Launcher.Commands.Templates;
using Reloaded.Mod.Launcher.Models.ViewModel;
using Reloaded.Mod.Loader.Update.Converters.NuGet;
using Reloaded.Mod.Shared;

namespace Reloaded.Mod.Launcher.Commands.ManageModsPage
{
    public class ConvertToNugetCommand : WithCanExecuteChanged, ICommand
    {
        private readonly ManageModsViewModel _manageModsViewModel;
        private bool _canExecute = true;

        public ConvertToNugetCommand()
        {
            _manageModsViewModel = IoC.Get<ManageModsViewModel>();
            _manageModsViewModel.PropertyChanged += ManageModsViewModelPropertyChanged;
        }

        ~ConvertToNugetCommand()
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


        /* Implementation */
        public bool CanExecute(object parameter)
        {
            if (_manageModsViewModel.SelectedModTuple != null)
            {
                string directoryPath = Path.GetDirectoryName(_manageModsViewModel.SelectedModTuple.ModConfigPath);
                if (Directory.Exists(directoryPath))
                    return _canExecute;
            }

            return false;
        }

        public void Execute(object parameter)
        {
            _canExecute = false;
            RaiseCanExecute(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));

            // Start
            var selectedMod      = _manageModsViewModel.SelectedModTuple;
            var modDirectory     = Path.GetDirectoryName(selectedMod.ModConfigPath);
            var outputPath       = Path.GetDirectoryName(modDirectory);
            var converter        = new Converter();
            converter.FromModDirectory(modDirectory, outputPath, selectedMod.ModConfig);
            ProcessExtensions.OpenFileWithExplorer(outputPath);

            // End
            _canExecute = true;
            RaiseCanExecute(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }
}