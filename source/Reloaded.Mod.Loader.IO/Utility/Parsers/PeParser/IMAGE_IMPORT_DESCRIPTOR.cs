namespace Reloaded.Mod.Loader.IO.Utility.Parsers.PeParser;

public struct IMAGE_IMPORT_DESCRIPTOR
{
    /// <summary>
    ///     Points to the first ImageImportByName struct.
    /// </summary>
    public uint OriginalFirstThunk;

    /// <summary>
    ///     Time and date stamp.
    /// </summary>
    public uint TimeDateStamp;

    /// <summary>
    ///     Forwarder Chain.
    /// </summary>
    public uint ForwarderChain;

    /// <summary>
    ///     RVA to the name of the DLL.
    /// </summary>
    public uint Name;

    /// <summary>
    ///     Points to an ImageImportByName struct or
    ///     to the address of the first function.
    /// </summary>
    public uint FirstThunk;

    public bool IsDummy()
    {
        return OriginalFirstThunk == 0 &&
               TimeDateStamp == 0 &&
               ForwarderChain == 0 &&
               Name == 0 &&
               FirstThunk == 0;
    }
}