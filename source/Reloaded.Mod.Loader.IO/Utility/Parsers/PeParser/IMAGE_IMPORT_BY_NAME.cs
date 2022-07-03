namespace Reloaded.Mod.Loader.IO.Utility.Parsers.PeParser;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public unsafe struct IMAGE_IMPORT_BY_NAME
{
    public short Hint;
    public fixed byte NameBytes[1];

    public string Name
    {
        get
        {
            fixed (byte* pNameBytes = NameBytes)
                return new string((sbyte*)pNameBytes);
        }
    }
}