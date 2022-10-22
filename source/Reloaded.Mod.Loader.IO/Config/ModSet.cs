namespace Reloaded.Mod.Loader.IO.Config;

[Equals(DoNotAddEqualityOperators = true)]
public class ModSet : ObservableObject, IConfig<ModSet>
{
    /* Class Members */
    public string[] EnabledMods { get; set; }

    public ModSet() { EnabledMods = EmptyArray<string>.Instance; }
    public ModSet(IApplicationConfig applicationConfig) => EnabledMods = applicationConfig.EnabledMods;

    /// <summary>
    /// Reads a <see cref="ModSet"/> from the hard disk and returns its contents.
    /// </summary>
    public static ModSet FromFile(string filePath) => ConfigReader<ModSet>.ReadConfiguration(filePath);

    /// <summary>
    /// Assigns the list of enabled mods to a given application config.
    /// </summary>
    public void ToApplicationConfig(IApplicationConfig config) => config.EnabledMods = EnabledMods;

    /// <summary>
    /// Saves the current mod collection to a given file path.
    /// </summary>
    public void Save(string filePath) => ConfigReader<ModSet>.WriteConfiguration(filePath, this);

    /// <inheritdoc />
    public void SanitizeConfig()
    {
        EnabledMods ??= EmptyArray<string>.Instance;
    }

    // Reflection-less JSON
    public static JsonTypeInfo<ModSet> GetJsonTypeInfo(out bool supportsSerialize)
    {
        supportsSerialize = true;
        return ModSetContext.Default.ModSet;
    }
    
    public JsonTypeInfo<ModSet> GetJsonTypeInfoNet5(out bool supportsSerialize) => GetJsonTypeInfo(out supportsSerialize);
}