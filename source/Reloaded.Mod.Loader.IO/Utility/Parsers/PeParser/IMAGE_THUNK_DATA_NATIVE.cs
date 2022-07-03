namespace Reloaded.Mod.Loader.IO.Utility.Parsers.PeParser;

[StructLayout(LayoutKind.Explicit)]
public struct IMAGE_THUNK_DATA_NATIVE : IThunk
{
    [FieldOffset(0)]
    public nuint ForwarderString;

    [FieldOffset(0)]
    public nuint Function;

    [FieldOffset(0)]
    public nuint Ordinal;

    [FieldOffset(0)]
    public nuint AddressOfData;

    /// <inheritdoc />
    public bool IsDummy => AddressOfData == 0;
}