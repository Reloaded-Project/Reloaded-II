using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Reloaded.Mod.Launcher.Commands.Templates;
using Reloaded.Mod.Launcher.Models.Model.DownloadModsPage;
using Reloaded.Mod.Launcher.Models.ViewModel;

namespace Reloaded.Mod.Launcher.Commands.DownloadModsPage
{
    public class DownloadModCommand : WithCanExecuteChanged, ICommand
    {
        private readonly DownloadModsViewModel _downloadModsViewModel;
        private readonly ManageModsViewModel   _manageModsViewModel;
        private bool _canExecute = true;

        /* Create & Dispose */
        public DownloadModCommand()
        {
            try
            {
                _downloadModsViewModel = IoC.Get<DownloadModsViewModel>();
                _manageModsViewModel = IoC.Get<ManageModsViewModel>();
                _downloadModsViewModel.PropertyChanged += DownloadModsPropertyChanged;
                _manageModsViewModel.Mods.CollectionChanged += ModsOnCollectionChanged;
            }
            catch (Exception ex)
            {
                // Probably no internet
            }
        }

        ~DownloadModCommand()
        {
            Dispose();
        }

        public void Dispose()
        {
            _downloadModsViewModel.PropertyChanged -= DownloadModsPropertyChanged;
            _manageModsViewModel.Mods.CollectionChanged -= ModsOnCollectionChanged;
            GC.SuppressFinalize(this);
        }

        /* Implementation */

        private void ModsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RaiseCanExecute(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        private void DownloadModsPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(_downloadModsViewModel.DownloadModEntry))
                RaiseCanExecute(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        /* ICommand. */

        public bool CanExecute(object parameter)
        {
            if (! _canExecute)
                return false;

            if (_downloadModsViewModel.DownloadModEntry == null)
            {
                _downloadModsViewModel.DownloadModStatus = DownloadModStatus.Default;
                return false;
            }

            if (_manageModsViewModel.Mods.Any(x => x.ModConfig.ModId == _downloadModsViewModel.DownloadModEntry.Id))
            {
                _downloadModsViewModel.DownloadModStatus = DownloadModStatus.AlreadyDownloaded;
                return false;
            }

            _downloadModsViewModel.DownloadModStatus = DownloadModStatus.Default;
            return true;
        }

        public async void Execute(object parameter)
        {
            _downloadModsViewModel.DownloadModStatus = DownloadModStatus.Downloading;
            _canExecute = false;
            RaiseCanExecute(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));

            await Update.DownloadPackagesAsync(new []{ _downloadModsViewModel.DownloadModEntry.Id }, false, false);

            _canExecute = true;
            RaiseCanExecute(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            _downloadModsViewModel.DownloadModStatus = DownloadModStatus.Default;
        }
    }
}
