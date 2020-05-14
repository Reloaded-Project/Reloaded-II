using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using PeNet;
using PeNet.Header.Pe;

namespace Reloaded.Mod.Shared
{
    public static class PeNetExtensions
    {
        /// <summary>
        /// Gets a name for a given import descriptor.
        /// </summary>
        /// <param name="descriptors">The individual import descriptor.</param>
        /// <param name="descriptorSource">The file from which the descriptor originated.</param>
        /// <param name="peStream">Stream currently pointed to the beginning of the PE file.</param>
        public static unsafe string[] GetNames(this ImageImportDescriptor[] descriptors, PeFile descriptorSource, Stream peStream)
        {
            var streamPosition = peStream.Position;
            var names = new string[descriptors.Length];

            for (int x = 0; x < descriptors.Length; x++)
            {
                peStream.Position = streamPosition;
                names[x] = GetName(descriptors[x], descriptorSource, peStream);
            }

            return names;
        }

        /// <summary>
        /// Gets a name for a given import descriptor.
        /// </summary>
        /// <param name="descriptor">The individual import descriptor.</param>
        /// <param name="descriptorSource">The file from which the descriptor originated.</param>
        /// <param name="peStream">Stream currently pointed to the beginning of the PE file.</param>
        public static unsafe string GetName(this ImageImportDescriptor descriptor, PeFile descriptorSource, Stream peStream)
        {
            if (!TryRvaToAbsoluteAddress(descriptorSource, descriptor.Name, out long nameAddress))
                throw new Exception("Failed to map RVA to absolute address.");

            peStream.Seek(nameAddress, SeekOrigin.Current);

            // This is technically unsafe to do due to doing multiple assumptions.
            // Within the case of our program here, this is ok, 260 characters is a reasonable length for a max DLL name. 
            // This in fact used to be the max path length on Windows.
            const int maxStringLength = 260;
            int stringLength = 0;

            Span<byte> chars = stackalloc byte[maxStringLength];
            for (int x = 0; x < maxStringLength; x++)
            {
                byte b = (byte) peStream.ReadByte();
                if (b == 0x00)
                    break;

                chars[x] = b;
                stringLength += 1;
            }

            return Encoding.ASCII.GetString(chars.Slice(0, stringLength));
        }

        /// <summary>
        /// Converts a "Relative Virtual Address" to absolute address.
        /// </summary>
        public static bool TryRvaToAbsoluteAddress(this PeFile peFile, long rva, out long absoluteAddress)
        {
            absoluteAddress = 0;
            if (rva == 0)
                return false;

            var sectionHeaders = peFile.ImageSectionHeaders;
            foreach (var header in sectionHeaders)
            {
                var startAddress = header.VirtualAddress;
                var endAddress   = header.VirtualAddress + header.VirtualSize;

                if (rva >= startAddress && rva < endAddress)
                {
                    absoluteAddress = (rva - header.VirtualAddress + header.PointerToRawData);
                    return true;
                }
            }

            return false;
        }
    }
}
