using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Reloaded.Memory.Streams;
using Reloaded.Mod.Loader.Utilities.PeParser;

namespace Reloaded.Mod.Loader.Utilities
{
    /// <summary>
    /// Basic parser of PE Portable Executable Format
    /// </summary>
    public class BasicPeParser
    {
        private readonly IMAGE_DOS_HEADER _dosHeader;
        private readonly IMAGE_FILE_HEADER _fileHeader;
        private readonly IMAGE_OPTIONAL_HEADER32 _optionalHeader32;
        private readonly IMAGE_OPTIONAL_HEADER64 _optionalHeader64;
        private readonly IMAGE_SECTION_HEADER[] _imageSectionHeaders;

        /// <summary>
        /// Determines if the file header is 32bit or not.
        /// </summary>
        public bool Is32BitHeader => (0x0100 & FileHeader.Characteristics) == 0x0100;

        /// <summary>
        /// Gets the timestamp from the file header
        /// </summary>
        public DateTime TimeStamp => new DateTime(1970, 1, 1).AddSeconds(FileHeader.TimeDateStamp);

        public IMAGE_DOS_HEADER         DosHeader => _dosHeader;
        public IMAGE_FILE_HEADER        FileHeader => _fileHeader;
        public IMAGE_OPTIONAL_HEADER32  OptionalHeader32 => _optionalHeader32;
        public IMAGE_OPTIONAL_HEADER64  OptionalHeader64 => _optionalHeader64;
        public IMAGE_SECTION_HEADER[]   ImageSectionHeaders => _imageSectionHeaders;

        /// <summary>
        /// Parses a given PE header file.
        /// </summary>
        /// <param name="exeStream">Stream which starts at the beginning of the file, the DOS header.</param>
        public BasicPeParser(Stream exeStream)
        {
            // Read in the DLL or EXE and get the timestamp.
            using (var reader = new BufferedStreamReader(exeStream, 4096))
            {
                reader.Read<IMAGE_DOS_HEADER>(out _dosHeader);
                reader.Seek(_dosHeader.e_lfanew, SeekOrigin.Begin);
                reader.Seek(sizeof(uint), SeekOrigin.Current); // NT Header Signature

                reader.Read<IMAGE_FILE_HEADER>(out _fileHeader);
                if (Is32BitHeader)
                    reader.Read<IMAGE_OPTIONAL_HEADER32>(out _optionalHeader32);
                else
                    reader.Read<IMAGE_OPTIONAL_HEADER64>(out _optionalHeader64);

                _imageSectionHeaders = new IMAGE_SECTION_HEADER[FileHeader.NumberOfSections];
                for (int x = 0; x < ImageSectionHeaders.Length; ++x)
                    reader.Read<IMAGE_SECTION_HEADER>(out ImageSectionHeaders[x], true);
            }
        }
    }
}
