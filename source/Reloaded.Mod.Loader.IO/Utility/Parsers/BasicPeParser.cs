namespace Reloaded.Mod.Loader.IO.Utility.Parsers;

/// <summary>
/// Basic parser of PE Portable Executable Format
/// </summary>
public class BasicPeParser : IDisposable
{
    private IMAGE_DOS_HEADER _dosHeader;
    private IMAGE_FILE_HEADER _fileHeader;
    private IMAGE_OPTIONAL_HEADER32 _optionalHeader32;
    private IMAGE_OPTIONAL_HEADER64 _optionalHeader64;
    private IMAGE_DATA_DIRECTORY[] _dataDirectories;
    private IMAGE_SECTION_HEADER[] _imageSectionHeaders;
    private IMAGE_IMPORT_DESCRIPTOR[] _importDescriptors;
    private bool _isMapped;

    /// <summary>
    /// Determines if the file header is 32bit or not.
    /// </summary>
    public bool Is32BitHeader => (0x0100 & FileHeader.Characteristics) == 0x0100;

    /// <summary>
    /// Gets the timestamp from the file header
    /// </summary>
    public DateTime TimeStamp => new DateTime(1970, 1, 1).AddSeconds(FileHeader.TimeDateStamp);

    /// <summary>
    /// Reader for the underlying stream this class was created with.
    /// </summary>
    public BufferedStreamReader<Stream> Stream { get; private set; }

    /// <summary>
    /// Start of the underlying stream the class was created with.
    /// </summary>
    public long StreamStart { get; private set; }

    public IMAGE_DOS_HEADER         DosHeader => _dosHeader;
    public IMAGE_FILE_HEADER        FileHeader => _fileHeader;
    public IMAGE_OPTIONAL_HEADER32  OptionalHeader32 => _optionalHeader32;
    public IMAGE_OPTIONAL_HEADER64  OptionalHeader64 => _optionalHeader64;
    public IMAGE_DATA_DIRECTORY[]   DataDirectories => _dataDirectories;
    public IMAGE_SECTION_HEADER[]   ImageSectionHeaders => _imageSectionHeaders;
    public IMAGE_IMPORT_DESCRIPTOR[] ImportDescriptors => _importDescriptors;
        

    /// <summary>
    /// Basic parser for a PE header. Not 100% specification complete, just parses what is needed for Reloaded's use case.
    /// </summary>
    /// <param name="filePath">Path to the file.</param>
    public BasicPeParser(string filePath)
    {
        _isMapped = false;
        FromStream(new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read));
    }

    /// <summary>
    /// Basic parser for a PE header. Not 100% specification complete, just parses what is needed for Reloaded's use case.
    /// </summary>
    /// <param name="stream">Stream which starts at the beginning of the file, the DOS header.</param>
    /// <param name="isMapped">True if the PE is mapped (has been loaded) into memory.</param>
    public BasicPeParser(Stream stream, bool isMapped)
    {
        _isMapped = isMapped;
        FromStream(stream);
    }

    ~BasicPeParser()
    {
        Dispose();
    }

    public void Dispose()
    {
        Stream?.Dispose();
        GC.SuppressFinalize(this);
    }

    private void FromStream(Stream stream)
    {
        // Read in the DLL or EXE and get the timestamp.
        Stream = new BufferedStreamReader<Stream>(stream, 4096);
        StreamStart = stream.Position;

        Stream.Read<IMAGE_DOS_HEADER>(out _dosHeader);
        Stream.Seek(_dosHeader.e_lfanew, SeekOrigin.Begin);
        Stream.Seek(sizeof(uint), SeekOrigin.Current); // NT Header Signature

        Stream.Read<IMAGE_FILE_HEADER>(out _fileHeader);
        bool isPe32 = Stream.Peek<ushort>() == 0x10B;

        if (isPe32)
        {
            Stream.Read<IMAGE_OPTIONAL_HEADER32>(out _optionalHeader32);
            PopulateDataDirectories(Stream, _optionalHeader32.NumberOfRvaAndSizes);
        }
        else
        {
            Stream.Read<IMAGE_OPTIONAL_HEADER64>(out _optionalHeader64);
            PopulateDataDirectories(Stream, _optionalHeader64.NumberOfRvaAndSizes);
        }

        _imageSectionHeaders = new IMAGE_SECTION_HEADER[FileHeader.NumberOfSections];
        for (int x = 0; x < ImageSectionHeaders.Length; ++x)
            ImageSectionHeaders[x] = Stream.ReadMarshalled<IMAGE_SECTION_HEADER>();

        PopulateImportDescriptors(Stream);
    }

    /// <summary>
    /// Converts a "Relative Virtual Address" to an absolute address.
    /// This is the address in memory, if we are reading a mapped PE; other
    /// </summary>
    public bool TryRvaToAbsoluteAddress(long rva, out long absoluteAddress)
    {
        // If it's already mapped into memory then RVA == Absolute
        if (_isMapped)
        {
            absoluteAddress = rva;
            return true;
        }

        // If it's not mapped, check what section it's in and adjust for pointer to raw data.
        absoluteAddress = 0;
        if (rva == 0)
            return false;

        var sectionHeaders = ImageSectionHeaders;
        foreach (var header in sectionHeaders)
        {
            var startAddress = header.VirtualAddress;
            var endAddress = header.VirtualAddress + header.VirtualSize;

            if (rva >= startAddress && rva < endAddress)
            {
                absoluteAddress = (rva - header.VirtualAddress + header.PointerToRawData);
                return true;
            }
        }

        return false;
    }

    /* Utility Functions */

    /// <summary>
    /// Gets a name for all import descriptors.
    /// </summary>
    public unsafe string[] GetImportDescriptorNames()
    {
        var names = new string[ImportDescriptors.Length];
        for (int x = 0; x < names.Length; x++)
        {
            names[x] = GetImportDescriptorName(ImportDescriptors[x]);
        }

        return names;
    }

    /// <summary>
    /// Gets a name for a given import descriptor.
    /// </summary>
    /// <param name="descriptor">The individual import descriptor.</param>
    public unsafe string GetImportDescriptorName(IMAGE_IMPORT_DESCRIPTOR descriptor)
    {
        if (!TryRvaToAbsoluteAddress(descriptor.Name, out long nameAddress))
            throw new Exception("Failed to map RVA to absolute address.");

        Stream.Seek(StreamStart, SeekOrigin.Begin);
        Stream.Seek(nameAddress, SeekOrigin.Current);

        // This is technically unsafe to do due to doing multiple assumptions.
        // Within the case of our program here, this is ok, 260 characters is a reasonable length for a max DLL name. 
        // This in fact used to be the max path length on Windows.
        const int maxStringLength = 260;
        int stringLength = 0;

        Span<byte> chars = stackalloc byte[maxStringLength];
        for (int x = 0; x < maxStringLength; x++)
        {
            var b = Stream.Read<byte>();
            if (b == 0x00)
                break;

            chars[x] = b;
            stringLength += 1;
        }

        return Encoding.ASCII.GetString(chars.Slice(0, stringLength));
    }

    /* Data Parsing */
    private void PopulateDataDirectories(BufferedStreamReader<Stream> reader, long numberOfEntries)
    {
        _dataDirectories = new IMAGE_DATA_DIRECTORY[numberOfEntries];
        for (int x = 0; x < numberOfEntries; x++)
            reader.Read<IMAGE_DATA_DIRECTORY>(out _dataDirectories[x]);
    }

    private unsafe void PopulateImportDescriptors(BufferedStreamReader<Stream> reader)
    {
        var directory = DataDirectories[(int) DataDirectoryType.ImportTable];
        var numberOfEntries = directory.Size / sizeof(IMAGE_IMPORT_DESCRIPTOR);
        var descriptors = new List<IMAGE_IMPORT_DESCRIPTOR>((int) numberOfEntries);
            
        reader.Seek(StreamStart, SeekOrigin.Begin);
        if (TryRvaToAbsoluteAddress(directory.VirtualAddress, out long absoluteAddress))
        {
            reader.Seek(absoluteAddress, SeekOrigin.Current);
            for (int x = 0; x < numberOfEntries; x++)
            {
                var descriptor = reader.Read<IMAGE_IMPORT_DESCRIPTOR>();
                if (!descriptor.IsDummy())
                    descriptors.Add(descriptor);
            }
        }

        _importDescriptors = descriptors.ToArray();
    }
}