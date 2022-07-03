namespace Reloaded.Mod.Loader.IO.Utility.Parsers.PeParser;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct IMAGE_FILE_HEADER
{
    public ushort Machine;
    public ushort NumberOfSections;
    public uint TimeDateStamp;
    public uint PointerToSymbolTable;
    public uint NumberOfSymbols;
    public ushort SizeOfOptionalHeader;
    public ushort Characteristics;
}