namespace Reloaded.AutoIndexBuilder.Events.Notification;

/// <summary>
/// Sends over a request to notify listeners that the build has finished.
/// </summary>
public class BuildFinishedNotification : INotification
{
    /// <summary>
    /// The duration taken by this operation.
    /// </summary>
    public TimeSpan Runtime { get; set; }

    /// <summary>
    /// The entry represented by this operation.
    /// </summary>
    public SourceEntry Entry { get; set; } = null!;
}