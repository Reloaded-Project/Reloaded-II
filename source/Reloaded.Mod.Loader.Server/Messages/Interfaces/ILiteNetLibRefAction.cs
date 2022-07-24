namespace Reloaded.Mod.Loader.Server.Messages.Interfaces;

/// <summary>
/// Alias for <see cref="IMsgRefAction{TStruct,TExtraData}"/>.
/// </summary>
public interface ILiteNetLibRefAction<TStruct> : IMsgRefAction<TStruct, LiteNetLibState> { }