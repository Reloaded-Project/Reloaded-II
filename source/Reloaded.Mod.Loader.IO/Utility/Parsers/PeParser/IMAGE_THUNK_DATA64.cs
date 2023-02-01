namespace Reloaded.Mod.Loader.IO.Utility.Parsers.PeParser;

[StructLayout(LayoutKind.Explicit)]
public struct IMAGE_THUNK_DATA64 : IThunk
{
    [FieldOffset(0)]
    public ulong ForwarderString;

    [FieldOffset(0)]
    public ulong Function;

    [FieldOffset(0)]
    public ulong Ordinal;

    [FieldOffset(0)]
    public ulong AddressOfData;

    /// <inheritdoc />
    public bool IsDummy => AddressOfData == 0;
}