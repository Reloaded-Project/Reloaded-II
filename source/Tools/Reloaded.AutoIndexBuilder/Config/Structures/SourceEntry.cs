namespace Reloaded.AutoIndexBuilder.Config.Structures;

public class SourceEntry : IndexSourceEntry
{
    /// <summary>
    /// Amount of minutes before this source should be updated again.
    /// </summary>
    public int MinutesBetweenRefresh { get; set; } = 60;

    /// <summary>
    /// Whether this service is enabled.
    /// </summary>
    public bool Enabled { get; set; } = false;

    /// <summary>
    /// A user friendly name for this entry.
    /// </summary>
    public string FriendlyName { get; set; } = "Sample Source";

    // Serialization
    public SourceEntry() { }
    public SourceEntry(long appId) : base(appId) { }
    public SourceEntry(string nuGetUrl) : base(nuGetUrl) { }
}