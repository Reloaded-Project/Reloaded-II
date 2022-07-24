namespace Reloaded.Mod.Loader.Server.Messages.Interfaces;

/// <summary>
/// Represents an interface that can be packed using `Reloaded.Messaging`.
/// </summary>
public interface IPackable
{
    /// <summary>
    /// Packs the current instance of the item.
    /// </summary>
    ReusableSingletonMemoryStream Pack();
}