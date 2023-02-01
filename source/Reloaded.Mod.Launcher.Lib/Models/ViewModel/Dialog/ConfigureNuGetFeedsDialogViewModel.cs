namespace Reloaded.Mod.Launcher.Lib.Models.ViewModel.Dialog;

/// <summary>
/// ViewModel for the dialog that allows the user to add and remove NuGet Feeds.
/// </summary>
public class ConfigureNuGetFeedsDialogViewModel : ObservableObject, IDisposable
{
    /// <summary>
    /// This command can be used to create a new feed.
    /// </summary>
    public CreateNewFeedCommand CreateNewFeedCommand { get; set; }

    /// <summary>
    /// This command can be used to delete an existing feed.
    /// </summary>
    public DeleteFeedCommand DeleteFeedCommand { get; set; }

    /// <summary>
    /// The currently selected feed in the user UI.
    /// </summary>
    public NugetFeed? CurrentFeed { get; set; }

    /// <summary>
    /// List of all available feeds for editing.
    /// </summary>
    public ObservableCollection<NugetFeed> Feeds { get; set; }

    /// <summary>
    /// True if the menu elements (feed elements) can be customized/changed.
    /// </summary>
    public bool IsEnabled => CurrentFeed != null;

    private readonly LoaderConfig _config;

    /// <inheritdoc />
    public ConfigureNuGetFeedsDialogViewModel(LoaderConfig loaderConfig)
    {
        _config = loaderConfig;
        Feeds = new ObservableCollection<NugetFeed>(loaderConfig.NuGetFeeds);
        CurrentFeed = Feeds.FirstOrDefault();
        CreateNewFeedCommand = new CreateNewFeedCommand(this);
        DeleteFeedCommand = new DeleteFeedCommand(this);
    }

    /// <inheritdoc />
    public void Dispose() => DeleteFeedCommand.Dispose();

    /// <summary>
    /// Writes the configuration to loader config and saves the config.
    /// </summary>
    public void Save()
    {
        _config.NuGetFeeds = Feeds.Where(x => !string.IsNullOrEmpty(x.URL)).ToArray();
        IConfig<LoaderConfig>.ToPath(_config, Paths.LoaderConfigPath);
        IoC.Get<AggregateNugetRepository>().FromFeeds(Feeds);
    }
}

/// <summary>
/// This command can be used to add a feed to the <see cref="ConfigureNuGetFeedsDialogViewModel"/>.
/// </summary>
public class CreateNewFeedCommand : ICommand
{
    private ConfigureNuGetFeedsDialogViewModel _viewModel;

    /// <summary/>
    public CreateNewFeedCommand(ConfigureNuGetFeedsDialogViewModel viewModel) => _viewModel = viewModel;

    /// <inheritdoc />
    public bool CanExecute(object? parameter) => true;

    /// <inheritdoc />
    public void Execute(object? parameter)
    {
        var feed = new NugetFeed("New NuGet Feed", "");
        _viewModel.Feeds.Add(feed);
        _viewModel.CurrentFeed = feed;
    }

    /// <inheritdoc />
    public event EventHandler? CanExecuteChanged;
}

/// <summary>
/// This command can be used to delete a feed from the <see cref="ConfigureNuGetFeedsDialogViewModel"/>.
/// </summary>
public class DeleteFeedCommand : ICommand, IDisposable
{
    private ConfigureNuGetFeedsDialogViewModel _viewModel;

    /// <summary/>
    public DeleteFeedCommand(ConfigureNuGetFeedsDialogViewModel viewModel)
    {
        _viewModel = viewModel;
        _viewModel.PropertyChanged += ViewModelOnPropertyChanged;
    }

    /// <inheritdoc />
    public void Dispose() => _viewModel.PropertyChanged -= ViewModelOnPropertyChanged;

    private void ViewModelOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(_viewModel.CurrentFeed))
            CanExecuteChanged?.Invoke(sender, e);
    }

    /// <inheritdoc />
    public bool CanExecute(object? parameter) => _viewModel.CurrentFeed != null;

    /// <inheritdoc />
    public void Execute(object? parameter)
    {
        if (_viewModel.CurrentFeed == null)
            return;

        _viewModel.Feeds.Remove(_viewModel.CurrentFeed);
        _viewModel.CurrentFeed = _viewModel.Feeds.FirstOrDefault();
    }

    /// <inheritdoc />
    public event EventHandler? CanExecuteChanged;
}