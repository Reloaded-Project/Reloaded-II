namespace Reloaded.AutoIndexBuilder.Config;

public class Stats
{
    /// <summary>
    /// Time the server has been started.
    /// </summary>
    [JsonIgnore]
    public DateTime StartTime { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Total number of builds executed in history of program.
    /// </summary>
    public long TotalBuilds { get; set; }

    /// <summary>
    /// Total number of builds executed since started.
    /// </summary>
    [JsonIgnore]
    public long BuildsSinceStarted { get; set; }

    /// <summary>
    /// Time the server has been started, in unix time.
    /// </summary>
    [JsonIgnore]
    public long StartTimeUnix => ((DateTimeOffset)StartTime).ToUnixTimeSeconds();

    /// <summary>
    /// Duration the server has been running live.
    /// </summary>
    [JsonIgnore]
    public TimeSpan Uptime => DateTime.UtcNow - StartTime;

    /// <summary>
    /// Gets the statistics, either from existing file or makes new ones,
    /// </summary>
    /// <returns></returns>
    public static Stats Get()
    {
        if (File.Exists(Paths.StatsPath))
            return JsonSerializer.Deserialize<Stats>(File.ReadAllText(Paths.StatsPath))!;

        return new Stats();
    }

    /// <summary>
    /// Writes the stats to configuration file in asynchronous fashion.
    /// </summary>
    public async Task WriteAsync()
    {
        await using FileStream statsFile = new FileStream(Paths.StatsPath, FileMode.OpenOrCreate);
        await JsonSerializer.SerializeAsync(statsFile, this);
    }
}