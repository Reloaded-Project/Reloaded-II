namespace Reloaded.Mod.Loader.Server.Messages.Interfaces;

/// <summary>
/// Represents an individual message that contains a key echoed from the request to the response.
/// Basically every message in this library.
/// </summary>
public interface IKeyedMessage
{
    /// <summary>
    /// Unique (on client end) key for this message.
    /// </summary>
    public MessageKey Key { get; set; }
}