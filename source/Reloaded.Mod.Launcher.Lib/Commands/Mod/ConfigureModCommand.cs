namespace Reloaded.Mod.Launcher.Lib.Commands.Mod;

/// <summary>
/// Command that allows you to configure an individual mod within Reloaded.
/// </summary>
public class ConfigureModCommand : WithCanExecuteChanged, ICommand
{
    private static Type[] _sharedTypes = { typeof(IConfiguratorV1) };
    private readonly PathTuple<ModConfig>? _modTuple;
    private readonly PathTuple<ModUserConfig>? _modUserConfigTuple;
    private readonly PathTuple<ApplicationConfig> _applicationTuple;
    private bool? _canExecute = null;

    /// <inheritdoc />
    public ConfigureModCommand(PathTuple<ModConfig>? modTuple, PathTuple<ModUserConfig>? userConfig, PathTuple<ApplicationConfig> applicationTuple)
    {
        _modTuple = modTuple;
        _modUserConfigTuple = userConfig;
        _applicationTuple = applicationTuple;
    }

    /* ICommand */

    // Disallowed inlining to ensure nothing from library can be kept alive by stack references etc.
    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.NoInlining)]
    public void Execute(object? parameter)
    {
        Execute_Internal();
        GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true, true);
    }

    // Disallowed inlining to ensure nothing from library can be kept alive by stack references etc.
    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.NoInlining)]
    public bool CanExecute(object? parameter)
    {
        if (_modTuple == null) 
            return false;

        if (_canExecute.HasValue)
            return _canExecute.Value;

        try
        {
            _canExecute = TryGetConfiguratorDisposing();
            return _canExecute.Value;
        }
        catch (Exception)
        {
            _canExecute = false;
            return _canExecute.Value;
        }
    }

    // Disallowed inlining to ensure nothing from library can be kept alive by stack references etc.
    [MethodImpl(MethodImplOptions.NoInlining)]
    private bool TryGetConfiguratorDisposing()
    {
        var result = TryGetConfigurator(out var configurator, out var loader);
        loader?.Dispose();
        return result;
    }

    // Disallowed inlining to ensure nothing from library can be kept alive by stack references etc.
    [MethodImpl(MethodImplOptions.NoInlining)]
    private bool TryGetConfigurator(out IConfiguratorV1? configurator, out PluginLoader? loader)
    {
        var config = _modTuple!.Config;
        string dllPath = config.GetManagedDllPath(_modTuple.Path);
        configurator = null;
        loader = null;

        if (!File.Exists(dllPath))
            return false;

        loader = PluginLoader.CreateFromAssemblyFile(dllPath, true, _sharedTypes, config =>
        {
            config.DefaultContext = AssemblyLoadContext.GetLoadContext(Assembly.GetExecutingAssembly())!;
            config.IsLazyLoaded = true;
            config.LoadInMemory = true;
        });

        var assembly = loader.LoadDefaultAssembly();
        var types = assembly.GetTypes();
        var entryPoint = types.FirstOrDefault(t => typeof(IConfiguratorV1).IsAssignableFrom(t) && !t.IsAbstract);
        
        if (entryPoint == null) 
            return false;

        configurator = (IConfiguratorV1)Activator.CreateInstance(entryPoint)!;
        var modDirectory = Path.GetFullPath(Path.GetDirectoryName(_modTuple.Path)!);
        configurator.SetModDirectory(modDirectory);

        if (configurator is IConfiguratorV2 versionTwo && _modUserConfigTuple != null)
        {
            var configDirectory = Path.GetFullPath(Path.GetDirectoryName(_modUserConfigTuple.Path)!);
            versionTwo.Migrate(modDirectory, configDirectory);
            versionTwo.SetConfigDirectory(configDirectory);
        }

        if (configurator is IConfiguratorV3 versionThree)
        {
            versionThree.SetContext(new ConfiguratorContext()
            {
                Application = _applicationTuple.Config,
                ModConfigPath = _modTuple.Path,
                ApplicationConfigPath = _applicationTuple.Path
            });
        }
            
        return true;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void Execute_Internal()
    {
        // Important Note: We are keeping everything to the stack.
        // Want our best to ensure that no types leak out anywhere making unloadability hard.
        // Also, we must also keep loader used to load the configurator in stack, for obvious reasons.
        if (!TryGetConfigurator(out var configurator, out _)) 
            return;

        if (configurator!.TryRunCustomConfiguration()) 
            return;

        Actions.ConfigureModDialog.Invoke(new ConfigureModDialogViewModel(configurator.GetConfigurations()));
    }
}