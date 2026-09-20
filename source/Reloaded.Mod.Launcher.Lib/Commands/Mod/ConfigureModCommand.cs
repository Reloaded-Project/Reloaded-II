using Native = Reloaded.Mod.Launcher.Lib.Models.Model.Configuration.Native;

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

    /// <summary>
    /// Full path of the mod's user config folder; null when the mod has none.
    /// </summary>
    private string? ExistingUserConfigFolder => _modUserConfigTuple != null
        ? Path.GetFullPath(Path.GetDirectoryName(_modUserConfigTuple.Path)!)
        : null;

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
        var modDirectory = Path.GetFullPath(Path.GetDirectoryName(_modTuple!.Path)!);

        // Native (non .NET) mods describe their settings in a schema file, no managed code required.
        if (Native.ModConfigSchema.ExistsInFolder(modDirectory))
        {
            loader = null;
            configurator = CreateNativeConfigurator(modDirectory);
            return true;
        }

        return TryGetManagedConfigurator(modDirectory, out configurator, out loader);
    }

    /// <summary>
    /// Creates the configurator for a native mod.
    /// </summary>
    /// <remarks>
    /// Throws when the settings schema is broken or the settings cannot move
    /// to the user config folder.
    /// </remarks>
    private IConfiguratorV1 CreateNativeConfigurator(string modDirectory)
    {
        // Validate upfront, a broken schema disables the button instead of failing later.
        Native.ModConfigSchema.Load(modDirectory);

        // Native settings always live in the user config folder, creating the
        // standard one when the mod has none yet.
        string configDirectory = ExistingUserConfigFolder
            ?? ModUserConfig.GetUserConfigFolderForMod(_modTuple!.Config.ModId);

        Directory.CreateDirectory(configDirectory);

        var nativeConfigurator = new Native.ModConfigurator(modDirectory);
        ConfigureConfigurator(nativeConfigurator, modDirectory, configDirectory);

        return nativeConfigurator;
    }

    /// <summary>
    /// Loads the configurator from the mod's .NET DLL, returning false when the
    /// DLL is missing or holds no configurator.
    /// </summary>
    private bool TryGetManagedConfigurator(string modDirectory, out IConfiguratorV1? configurator, out PluginLoader? loader)
    {
        var config = _modTuple!.Config;
        configurator = null;
        loader = null;

        string dllPath = config.GetManagedDllPath(_modTuple.Path);

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
        ConfigureConfigurator(configurator, modDirectory, ExistingUserConfigFolder);

        return true;
    }

    /// <summary>
    /// Sets up a freshly created configurator with its mod directory, user
    /// config location and application context.
    /// </summary>
    /// <param name="configurator">The configurator to set up.</param>
    /// <param name="modDirectory">Full path to the mod's folder.</param>
    /// <param name="configDirectory">Full path to the mod's user config
    /// folder; null skips migration and leaves the location untouched.</param>
    private void ConfigureConfigurator(IConfiguratorV1 configurator, string modDirectory, string? configDirectory)
    {
        configurator.SetModDirectory(modDirectory);

        if (configurator is IConfiguratorV2 versionTwo && configDirectory != null)
        {
            MigrateConfigurator(versionTwo, modDirectory, configDirectory);
            versionTwo.SetConfigDirectory(configDirectory);
        }

        if (configurator is IConfiguratorV3 versionThree)
            versionThree.SetContext(CreateContext());
    }

    /// <summary>
    /// Moves a configurator's config files to a new folder.
    /// </summary>
    private void MigrateConfigurator(IConfiguratorV2 configurator, string modDirectory, string configDirectory)
    {
        if (configurator is Native.ModConfigurator native)
        {
            if (!native.TryMigrate(modDirectory, configDirectory))
                throw new InvalidOperationException($"Could not move the settings of '{_modTuple!.Config.ModName}' from '{modDirectory}' to '{configDirectory}'.", native.MigrationError);
        }
        else
        {
            configurator.Migrate(modDirectory, configDirectory);
        }
    }

    /// Builds the application/mod context handed to V3 configurators.
    private ConfiguratorContext CreateContext() => new ConfiguratorContext()
    {
        Application = _applicationTuple.Config,
        ModConfigPath = _modTuple!.Path,
        ApplicationConfigPath = _applicationTuple.Path
    };

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