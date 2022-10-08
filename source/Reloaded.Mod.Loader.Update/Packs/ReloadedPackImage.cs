namespace Reloaded.Mod.Loader.Update.Packs;

/// <summary>
/// Represents a singular image stored inside the Reloaded package.
/// </summary>
[Equals(DoNotAddEqualityOperators = true)]
public struct ReloadedPackImage
{
    // TODO: [NET7] Restore constructor with guarantee of non-null username. Right now we can't because it breaks built-in System.Text.Json. This is fixed in newer versions.

    /// <summary>
    /// Path of the image inside the archive.
    /// </summary>
    public string Path { get; set; }

    /// <summary>
    /// Caption of the image.
    /// </summary>
    public string? Caption { get; set; }
}