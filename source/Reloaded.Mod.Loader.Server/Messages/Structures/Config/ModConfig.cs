namespace Reloaded.Mod.Loader.Server.Messages.Structures.Config;

/// <summary>
/// Reference mod configuration.
/// </summary>
[Equals(DoNotAddEqualityOperators = true, DoNotAddGetHashCode = true)]
public struct ModConfig : IModConfig
{
    /// <inheritdoc/>
    public string ModId { get; set; } = "";

    /// <inheritdoc/>
    public string ModName { get; set; } = "";

    /// <inheritdoc/>
    public string ModAuthor { get; set; } = "";

    /// <inheritdoc/>
    public string ModVersion { get; set; } = "";

    /// <inheritdoc/>
    public string ModDescription { get; set; } = "";

    /// <inheritdoc/>
    public string ModDll { get; set; } = "";

    /// <inheritdoc/>
    public string ModIcon { get; set; } = "";

    /// <inheritdoc/>
    public string ModR2RManagedDll32 { get; set; } = "";

    /// <inheritdoc/>
    public string ModR2RManagedDll64 { get; set; } = "";

    /// <inheritdoc/>
    public string ModNativeDll32 { get; set; } = "";

    /// <inheritdoc/>
    public string ModNativeDll64 { get; set; } = "";

    /// <inheritdoc/>
    public bool IsLibrary { get; set; } = false;

    /// <inheritdoc/>
    public string ReleaseMetadataFileName { get; set; } = "";

    /// <inheritdoc/>
    [IgnoreDuringEquals]
    public Dictionary<string, object>? PluginData { get; set; } = null!;

    /// <inheritdoc/>
    public bool IsUniversalMod { get; set; } = false;

    /// <inheritdoc/>
    public string[] ModDependencies { get; set; } = Array.Empty<string>();

    /// <inheritdoc/>
    public string[] OptionalDependencies { get; set; } = Array.Empty<string>();

    /// <inheritdoc/>
    public string[] SupportedAppId { get; set; } = Array.Empty<string>();

    public ModConfig() { }

    /// <inheritdoc/>
    public override int GetHashCode() => (ModId != null ? ModId.GetHashCode() : 0);
}