namespace Reloaded.Mod.Loader.Server.Messages.Structures;

public struct MessageKey
{
    /// <summary>
    /// The key that uniquely identifies this message.
    /// Allows max parallelism of 65536 messages from 1 client at a time.
    /// </summary>
    public ushort Key { get; set; }

    /// <summary>
    /// Constructs a server unique message key.
    /// </summary>
    public MessageKey(ushort key) => Key = key;

    public static implicit operator ushort(MessageKey k) => k.Key;
    public static implicit operator MessageKey(ushort key) => new MessageKey(key);
}