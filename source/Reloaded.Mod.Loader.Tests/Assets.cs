namespace Reloaded.Mod.Loader.Tests;

internal class Assets
{
    /// <summary>
    /// The location where the current program is located.
    /// </summary>
    public static readonly string ProgramFolder = Path.GetDirectoryName(new Uri(AppContext.BaseDirectory).LocalPath);

    /// <summary>
    /// Path to the folder containing the assets.
    /// </summary>
    public static readonly string AssetsFolder = Path.Combine(ProgramFolder, "Assets");

    /// <summary>
    /// Path to the folder containing the index.
    /// </summary>
    public static readonly string IndexAssetsFolder = Path.Combine(AssetsFolder, "Index");
    
    /// <summary>
    /// Path to the folder containing the r2 bundle package files.
    /// </summary>
    public static readonly string PackAssetsFolder = Path.Combine(AssetsFolder, "R2Pack");

    /// <summary>
    /// Path to the folder containing the sample index.
    /// </summary>
    public static readonly string SampleIndexAssetsFolder = Path.Combine(IndexAssetsFolder, "Sample");

    /// <summary>
    /// Path to the folder containing the sample index, with GameBanana results only.
    /// </summary>
    public static readonly string GameBananaIndexAssetsFolder = Path.Combine(IndexAssetsFolder, "GameBananaOnly");
}