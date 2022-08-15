namespace Reloaded.Mod.Loader.Update.Index.Structures.Config;

/// <summary>
/// Contains an individual entry used by the source builder to.
/// </summary>
public class IndexSourceEntry
{
    /// <summary>
    /// Type of the index to set.
    /// </summary>
    [JsonIgnore]
    public IndexType Type => GetIndexType();

    /// <summary>
    /// GameBanana ID for this item.
    /// </summary>
    public long? GameBananaId { get; set; }

    /// <summary>
    /// NuGet URL for this item.
    /// </summary>
    public string? NuGetUrl { get; set; }

    /// <summary>
    /// Serialization only.
    /// </summary>
    public IndexSourceEntry() {}

    /// <summary>
    /// Creates a source entry given a GameBanana application id.
    /// </summary>
    /// <param name="appId">Application id.</param>
    public IndexSourceEntry(long appId) => GameBananaId = appId;

    /// <summary>
    /// Creates a source entry given a NuGet URL.
    /// </summary>
    /// <param name="nuGetUrl">NuGet URL.</param>
    public IndexSourceEntry(string nuGetUrl) => NuGetUrl = nuGetUrl;

    /// <summary>
    /// Decodes the index type based on the string.
    /// </summary>
    public IndexType GetIndexType()
    {
        if (GameBananaId.HasValue)
            return IndexType.GameBanana;

        if (!string.IsNullOrEmpty(NuGetUrl))
            return IndexType.NuGet;

        return IndexType.Unknown;
    }
}

/// <summary>
/// Type of index used.
/// </summary>
public enum IndexType
{
#pragma warning disable CS1591
    Unknown,
    GameBanana,
    NuGet
#pragma warning restore CS1591
}