using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using NuGet.Protocol.Core.Types;
using Reloaded.Mod.Launcher.Lib.Commands.Download;
using Reloaded.Mod.Launcher.Lib.Commands.Templates;
using Reloaded.Mod.Launcher.Lib.Models.Model.DownloadPackagePage;
using Reloaded.Mod.Loader.IO.Services;
using Reloaded.Mod.Loader.IO.Utility;
using Reloaded.Mod.Loader.Update.Utilities.Nuget;
using Reloaded.Mod.Loader.Update.Utilities.Nuget.Structs;

namespace Reloaded.Mod.Launcher.Lib.Models.ViewModel;

/// <summary>
/// ViewModel for downloading packages from NuGet sources.
/// </summary>
public class DownloadPackagesViewModel : ObservableObject, IDisposable
{
    /// <summary>
    /// String to search mods by.
    /// </summary>
    public string SearchQuery { get; set; } = "";

    /// <summary>
    /// List of potential packages to download.
    /// </summary>
    public ObservableCollection<DownloadPackageEntry> DownloadModEntries  { get; set; }

    /// <summary>
    /// List of potential packages 
    /// </summary>
    public DownloadPackageEntry? DownloadPackageEntry { get; set; }

    /// <summary>
    /// Status of the current package download.
    /// </summary>
    public DownloadPackageStatus DownloadPackageStatus { get; set; }

    /// <summary>
    /// Command used to download an individual mod.
    /// </summary>
    public DownloadPackageCommand DownloadModCommand { get; set; }

    /// <summary>
    /// Command used to configure sources for NuGet packages.
    /// </summary>
    public ConfigureNuGetSourcesCommand ConfigureNuGetSourcesCommand { get; set; }

    private AggregateNugetRepository _nugetRepository;
    private CancellationTokenSource? _tokenSource;

    /* Construction - Deconstruction */

    /// <inheritdoc />
    public DownloadPackagesViewModel(AggregateNugetRepository nugetRepository)
    {
        _nugetRepository = nugetRepository;
        DownloadModEntries = new ObservableCollection<DownloadPackageEntry>();
        DownloadModCommand = new DownloadPackageCommand(this);
        ConfigureNuGetSourcesCommand = new ConfigureNuGetSourcesCommand(RefreshOnSourceChange);
        PropertyChanged += OnSearchQueryChanged;
    }

    /// <summary>
    /// Gets the search results for the current search term.
    /// </summary>
    /// <returns></returns>
    public async Task GetSearchResults()
    {
        _tokenSource?.Cancel();
        _tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(30));

        var searchTuples = await _nugetRepository.Search(SearchQuery, false, 50, _tokenSource.Token);
        if (!_tokenSource.Token.IsCancellationRequested)
        {
            var modEntries = new List<DownloadPackageEntry>();
            foreach (var tuple in searchTuples)
                modEntries.AddRange(tuple.Generic.Select(x => new DownloadPackageEntry(x, tuple.Repository)));

            Collections.ModifyObservableCollection(DownloadModEntries, modEntries);
        }
    }

    private void OnSearchQueryChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SearchQuery))
#pragma warning disable 4014
            GetSearchResults(); // Fire and forget.
#pragma warning restore 4014
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _tokenSource?.Dispose();
        DownloadModCommand.Dispose();
    }

    private async void RefreshOnSourceChange() => await GetSearchResults();
}

/// <summary>
/// Command allowing you to download an individual mod.
/// </summary>
public class DownloadPackageCommand : WithCanExecuteChanged, ICommand, IDisposable
{
    private readonly DownloadPackagesViewModel _downloadPackagesViewModel;
    private readonly ModConfigService _modConfigService;
    private bool _canExecute = true;
    
    /// <inheritdoc />
    public DownloadPackageCommand(DownloadPackagesViewModel downloadPackagesViewModel)
    {
        _downloadPackagesViewModel = downloadPackagesViewModel;
        _modConfigService = IoC.Get<ModConfigService>();

        try
        {
            _downloadPackagesViewModel.PropertyChanged += OnSelectedPackageChanged;
            _modConfigService.Items.CollectionChanged += ModsOnCollectionChanged;
        }
        catch (Exception)
        {
            // Probably no internet
        }
    }

    /// <summary/>
    ~DownloadPackageCommand() => Dispose();

    /// <inheritdoc />
    public void Dispose()
    {
        _downloadPackagesViewModel.PropertyChanged -= OnSelectedPackageChanged;
        _modConfigService.Items.CollectionChanged -= ModsOnCollectionChanged;
        GC.SuppressFinalize(this);
    }

    /* Implementation */

    private void ModsOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        RaiseCanExecute(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    private void OnSelectedPackageChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(_downloadPackagesViewModel.DownloadPackageEntry))
            RaiseCanExecute(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    /* ICommand. */

    /// <inheritdoc />
    public bool CanExecute(object? parameter)
    {
        if (!_canExecute)
            return false;

        if (_downloadPackagesViewModel.DownloadPackageEntry == null)
            return ReturnResult(false, DownloadPackageStatus.Default);

        if (_modConfigService.Items.Any(x => x.Config.ModId == _downloadPackagesViewModel.DownloadPackageEntry.Id))
            return ReturnResult(false, DownloadPackageStatus.AlreadyDownloaded);

        return ReturnResult(true, DownloadPackageStatus.Default);

        bool ReturnResult(bool canExecute, DownloadPackageStatus status)
        {
            _downloadPackagesViewModel.DownloadPackageStatus = status;
            return canExecute;
        }
    }

    /// <inheritdoc />
    public async void Execute(object? parameter)
    {
        _downloadPackagesViewModel.DownloadPackageStatus = DownloadPackageStatus.Downloading;
        _canExecute = false;
        RaiseCanExecute(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));

        var entry = _downloadPackagesViewModel.DownloadPackageEntry;
        var newest = Nuget.GetNewestVersion(await entry.Source.GetPackageDetails(entry.Id, false, false));
        var tuple = new NugetTuple<IPackageSearchMetadata>(entry.Source, newest);
        await Update.DownloadNuGetPackagesAsync(tuple, new List<string>(), false, false);

        _canExecute = true;
        RaiseCanExecute(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        _downloadPackagesViewModel.DownloadPackageStatus = DownloadPackageStatus.Default;
    }
}