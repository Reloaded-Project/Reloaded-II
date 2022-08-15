namespace Reloaded.AutoIndexBuilder;

internal class Paths
{
    /// <summary>
    /// The location where the current program is located.
    /// </summary>
    public static readonly string ProgramFolder = Path.GetDirectoryName(new Uri(AppContext.BaseDirectory).LocalPath)!;

    /// <summary>
    /// Path to the folder containing the main config.
    /// </summary>
    public static readonly string ConfigPath = Path.Combine(ProgramFolder, "Config.json");

    /// <summary>
    /// Path to the folder containing the stats.
    /// </summary>
    public static readonly string StatsPath = Path.Combine(ProgramFolder, "Stats.json");

}