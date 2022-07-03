namespace Reloaded.Mod.Loader.IO.Utility.Parsers.PeParser.Interfaces;

public interface IThunk
{
    /// <summary>
    /// True if this thunk is a dummy and signals the end of the list; else false.
    /// </summary>
    bool IsDummy { get; }
}