using ApplicationSubPage = Reloaded.Mod.Launcher.Lib.Models.Model.Pages.ApplicationSubPage;

namespace Reloaded.Mod.Launcher.Lib.Models.ViewModel;

/// <summary>
/// Represents the ViewModel for displaying an individual application page.
/// An application page consists of the main sidebar for the application (typically left) and a sub
/// page denoted by <see cref="ApplicationSubPage"/>.
/// </summary>
public class ApplicationViewModel : ObservableObject, IDisposable
{
    /// <summary>
    /// Executes once list of mods for this app refreshes.
    /// </summary>
    public event Action OnGetModsForThisApp = () => { };

    /// <summary>
    /// Executed when a new Mod Set is loaded by the user.
    /// </summary>
    public event Action OnLoadModSet = () => { };

    /// <summary>
    /// The application being currently viewed or edited.
    /// </summary>
    public PathTuple<ApplicationConfig> ApplicationTuple { get; }

    /// <summary>
    /// List of all of the mods for this application.
    /// </summary>
    public ObservableCollection<PathTuple<ModConfig>> ModsForThisApp { get; private set; } = new ObservableCollection<PathTuple<ModConfig>>();

    /// <summary>
    /// List of all processes for this application that are currently running Reloaded inside.
    /// </summary>
    public ObservableCollection<Process> ProcessesWithReloaded    { get; private set; } = new ObservableCollection<Process>();

    /// <summary>
    /// List of all processes for this application that are not running Reloaded inside.
    /// </summary>
    public ObservableCollection<Process> ProcessesWithoutReloaded { get; private set; } = new ObservableCollection<Process>();

    /// <summary>
    /// The current page to display on the right hand side.
    /// </summary>
    public ApplicationSubPage Page { get; private set; }

    /// <summary>
    /// The currently selected process.
    /// </summary>
    public Process? SelectedProcess { get; set; }

    /// <summary>
    /// Command allowing you to create a shortcut.
    /// </summary>
    public MakeShortcutCommand MakeShortcutCommand { get; set; } = null!;

    /// <summary>
    /// The number of mods available for this app.
    /// </summary>
    public int NumModsForThisApp { get; set; }

    private Timer? _refreshProcessesWithLoaderTimer = null;
    private ModConfigService _modConfigService;
    private ApplicationInstanceTracker _instanceTracker;
    private ModUserConfigService _modUserConfigService;
    private CancellationTokenSource _initialiseTokenSrc = new CancellationTokenSource();

    /// <inheritdoc />
    public ApplicationViewModel(PathTuple<ApplicationConfig> tuple, ModConfigService modConfigService, ModUserConfigService modUserConfigService, LoaderConfig loaderConfig)
    {
        ApplicationTuple    = tuple;
        _modConfigService    = modConfigService;
        _modUserConfigService = modUserConfigService;
        _instanceTracker = new ApplicationInstanceTracker(ApplicationConfig.GetAbsoluteAppLocation(tuple), _initialiseTokenSrc.Token);

        if (_initialiseTokenSrc.IsCancellationRequested)
            return;

        MakeShortcutCommand = new MakeShortcutCommand(tuple, Lib.IconConverter);
        _modConfigService.Items.CollectionChanged += OnGetModifications;
        _instanceTracker.OnProcessesChanged += OnProcessesChanged;

        UpdateReloadedProcesses();
        GetModsForThisApp();
        _refreshProcessesWithLoaderTimer = new Timer(RefreshTimerCallback, null, 500, loaderConfig.ProcessRefreshInterval);
    }

    /// <inheritdoc />
    ~ApplicationViewModel()
    {
        Dispose();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _initialiseTokenSrc.Cancel();
        _modConfigService.Items.CollectionChanged -= OnGetModifications;
        _refreshProcessesWithLoaderTimer?.Dispose();
        _instanceTracker.OnProcessesChanged -= OnProcessesChanged;
        _instanceTracker.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Changes the currently opened page in the application submenu.
    /// </summary>
    public void ChangeApplicationPage(ApplicationSubPage page) => Page = page;

    /// <summary>
    /// Allows the user to save a mod set.
    /// </summary>
    public void SaveModSet()
    {
        var dialog = new VistaSaveFileDialog { Title = Resources.SaveModSetDialogTitle.Get(), Filter = Constants.DialogJsonFilter, AddExtension = true, DefaultExt = ".json" };
        if ((bool)dialog.ShowDialog()!)
            new ModSet(ApplicationTuple.Config).Save(dialog.FileName);
    }

    /// <summary>
    /// Allows a user to load a new mod set.
    /// </summary>
    public async void LoadModSet()
    {
        var dialog = new VistaOpenFileDialog { Title = Resources.LoadModSetDialogTitle.Get(), Filter = Constants.DialogJsonFilter, AddExtension = true, DefaultExt = ".json" };
        if ((bool)dialog.ShowDialog()!)
        {
            ModSet.FromFile(dialog.FileName).ToApplicationConfig(ApplicationTuple.Config);
            await ApplicationTuple.SaveAsync();
                
            // Check for mod updates/dependencies.
            var deps = Update.CheckMissingDependencies();
            if (!deps.AllAvailable)
            {
                try { await Update.ResolveMissingPackagesAsync(); }
                catch (Exception) { /* ignored */ }
            }

            EnforceModCompatibility();
            OnLoadModSet();
        }
    }

    /// <summary>
    /// Enforces that all mods enabled for this application are automatically made compatible.
    /// </summary>
    public void EnforceModCompatibility() => Setup.EnforceModCompatibility(ApplicationTuple, _modConfigService.Items);

    // == Events ==
    private void RefreshTimerCallback(object? state) => UpdateReloadedProcesses();

    [SuppressPropertyChangedWarnings]
    private void OnProcessesChanged(Process[]? processes)
    {
        if (processes != null)
            UpdateReloadedProcesses();
    }

    private void OnGetModifications(object? sender, NotifyCollectionChangedEventArgs e) => GetModsForThisApp();

    private void UpdateReloadedProcesses()
    {
        var result = _instanceTracker.GetProcesses(_initialiseTokenSrc.Token);
        ActionWrappers.ExecuteWithApplicationDispatcherAsync(() =>
        {
            if (_initialiseTokenSrc.IsCancellationRequested)
                return;

            Collections.ModifyObservableCollection(ProcessesWithReloaded, result.ReloadedProcesses);
            Collections.ModifyObservableCollection(ProcessesWithoutReloaded, result.NonReloadedProcesses);
        });
    }

    private void GetModsForThisApp()
    {
        string appId = ApplicationTuple.Config.AppId;

        var newMods = new List<PathTuple<ModConfig>>();
        foreach (var modItem in _modConfigService.Items)
        {
            // Check config
            if (modItem.Config.IsUniversalMod || (modItem.Config.SupportedAppId != null && modItem.Config.SupportedAppId.Contains(appId)))
                goto addMod;

            // Check user config.
            if (!_modUserConfigService.ItemsById.TryGetValue(modItem.Config.ModId, out var modUserConfig)) 
                continue;

            if (!modUserConfig.Config.IsUniversalMod) 
                continue;
            
            addMod:
            newMods.Add(modItem);
        }

        // Modify collection.
        ActionWrappers.ExecuteWithApplicationDispatcherAsync(() =>
        {
            if (_initialiseTokenSrc.IsCancellationRequested)    
                return;

            Collections.ModifyObservableCollection(ModsForThisApp, newMods);
            NumModsForThisApp = ModsForThisApp.Count;
            OnGetModsForThisApp();
        });
    }
}