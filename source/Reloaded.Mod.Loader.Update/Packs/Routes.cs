namespace Reloaded.Mod.Loader.Update.Packs;

/// <summary>
/// Locations of items inside the Reloaded package.
/// </summary>
public class Routes
{
    /// <summary>
    /// Folder containing the images.
    /// </summary>
    public const string Images = "Images";

    /// <summary>
    /// Path of configuration inside the pack.
    /// </summary>
    public const string Config = "Config.json";

    /// <summary>
    /// Gets path to the image inside the R2 package.
    /// </summary>
    /// <param name="fileName">Name of the file.</param>
    public static string GetImagePath(string fileName) => $"{Images}/{fileName}";
}

/// <summary>
/// Other constants related to Reloaded mod packages.
/// </summary>
public class Constants
{
    /// <summary>
    /// File extension, including dot. 
    /// </summary>
    public const string Extension = ".r2pack";
}