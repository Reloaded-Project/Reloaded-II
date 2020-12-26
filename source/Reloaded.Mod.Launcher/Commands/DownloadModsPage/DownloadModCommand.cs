using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using NuGet.Protocol.Core.Types;
using Reloaded.Mod.Launcher.Commands.Templates;
using Reloaded.Mod.Launcher.Models.Model.DownloadModsPage;
using Reloaded.Mod.Launcher.Models.ViewModel;
using Reloaded.Mod.Loader.IO.Services;
using Reloaded.Mod.Loader.Update.Utilities.Nuget;
using Reloaded.Mod.Loader.Update.Utilities.Nuget.Structs;

namespace Reloaded.Mod.Launcher.Commands.DownloadModsPage
{
    public class DownloadModCommand : WithCanExecuteChanged, ICommand, IDisposable
    {
        private readonly DownloadModsViewModel _downloadModsViewModel;
        private readonly ModConfigService _modConfigService;
        private bool _canExecute = true;

        /* Create & Dispose */
        public DownloadModCommand(DownloadModsViewModel downloadModsViewModel)
        {
            try
            {
                _downloadModsViewModel = downloadModsViewModel;
                _modConfigService = IoC.Get<ModConfigService>();

                _downloadModsViewModel.PropertyChanged += OnSelectedModChanged;
                _modConfigService.Mods.CollectionChanged += ModsOnCollectionChanged;
            }
            catch (Exception)
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
            _downloadModsViewModel.PropertyChanged -= OnSelectedModChanged;
            _modConfigService.Mods.CollectionChanged -= ModsOnCollectionChanged;
            GC.SuppressFinalize(this);
        }

        /* Implementation */

        private void ModsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RaiseCanExecute(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        private void OnSelectedModChanged(object sender, PropertyChangedEventArgs e)
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
                return ReturnResult(false, DownloadModStatus.Default);

            if (_modConfigService.Mods.Any(x => x.Config.ModId == _downloadModsViewModel.DownloadModEntry.Id))
                return ReturnResult(false, DownloadModStatus.AlreadyDownloaded);

            return ReturnResult(true, DownloadModStatus.Default);

            bool ReturnResult(bool canExecute, DownloadModStatus status)
            {
                _downloadModsViewModel.DownloadModStatus = status;
                return canExecute;
            }
        }

        public async void Execute(object parameter)
        {
            _downloadModsViewModel.DownloadModStatus = DownloadModStatus.Downloading;
            _canExecute = false;
            RaiseCanExecute(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));

            var entry  = _downloadModsViewModel.DownloadModEntry;
            var newest = Nuget.GetNewestVersion(await entry.Source.GetPackageDetails(entry.Id, false, false));
            var tuple  = new NugetTuple<IPackageSearchMetadata>(entry.Source, newest);
            await Update.DownloadNuGetPackagesAsync(tuple, new List<string>(), false, false);

            _canExecute = true;
            RaiseCanExecute(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            _downloadModsViewModel.DownloadModStatus = DownloadModStatus.Default;
        }
    }
}
