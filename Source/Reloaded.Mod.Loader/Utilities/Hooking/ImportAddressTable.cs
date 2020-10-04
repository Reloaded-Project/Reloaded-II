using System;
using System.IO;
using Reloaded.Mod.Shared.PeParser;
using Kernel32 = Reloaded.Mod.Loader.Utilities.Native.Kernel32;

namespace Reloaded.Mod.Loader.Utilities.Hooking
{
    public unsafe class ImportAddressTable
    {
        /// <summary>
        /// Attempts to find for a given library.
        /// </summary>
        /// <param name="libraryName">The name of the library, including extension.</param>
        /// <param name="functionName">The name of the function in question.</param>
        /// <param name="address">The address of the function pointer in the import address table.</param>
        public static bool TryGetFunctionPtrAddress(string libraryName, string functionName, out nint* address)
        {
            const int imageDirectoryImportIndex = 1;
            address = (nint*)0;

            var cleanLibraryName      = Path.GetFileName(libraryName);

            var imageBase             = Kernel32.GetModuleHandle(null);
            var dosHeaders            = (IMAGE_DOS_HEADER*)imageBase;
            var optionalHeaderSize    = IntPtr.Size == 8 ? sizeof(IMAGE_OPTIONAL_HEADER64) : sizeof(IMAGE_OPTIONAL_HEADER32);
            var imageDataDirectoryPtr = (IMAGE_DATA_DIRECTORY*)(imageBase + (int)dosHeaders->e_lfanew + sizeof(uint) + sizeof(IMAGE_FILE_HEADER) + optionalHeaderSize);

            // sizeof(uint) == NT Header Signature
            // We just have skipped IMAGE_OPTIONAL_HEADER and IMAGE_FILE_HEADER to avoid having separate 32 and 64 bit paths.

            var importsDirectory      = &(imageDataDirectoryPtr[imageDirectoryImportIndex]);
            var importDescriptor      = (IMAGE_IMPORT_DESCRIPTOR*)((long)imageBase + importsDirectory->VirtualAddress);

            if (!TryFindDescriptorWithName(importDescriptor, imageBase, cleanLibraryName, out var libraryDescriptor))
                return false;

            // Found the correct import descriptor, let's load the library to ensure latest version is loaded and FirstThunk is populated.
            var importLibrary = Kernel32.LoadLibrary(cleanLibraryName);
            if (importLibrary == IntPtr.Zero)
                return false;

            var originalFirstThunk = (IMAGE_THUNK_DATA_NATIVE*)((long)imageBase + libraryDescriptor->OriginalFirstThunk);
            var firstThunk         = (IMAGE_THUNK_DATA_NATIVE*)((long)imageBase + libraryDescriptor->FirstThunk);

            while (originalFirstThunk->AddressOfData != 0) 
            {
                var importFunctionName = (IMAGE_IMPORT_BY_NAME*)((ulong)imageBase + originalFirstThunk->AddressOfData);
                if (importFunctionName->Name == functionName)
                {
                    address = (nint*)(&firstThunk->Function);
                    return true;
                }

                originalFirstThunk++;
                firstThunk++;
            }

            return false;
        }

        /// <summary>
        /// Tries to find an image descriptor which matches a given name.
        /// </summary>
        /// <param name="descriptor">Pointer to the first import descriptor.</param>
        /// <param name="imageBase">Base address of the module. 0 if file, else GetModuleHandle</param>
        /// <param name="libraryName">The name to find.</param>
        /// <param name="result">Pointer to the descriptor.</param>
        private static bool TryFindDescriptorWithName(IMAGE_IMPORT_DESCRIPTOR* descriptor, IntPtr imageBase, string libraryName, out IMAGE_IMPORT_DESCRIPTOR* result)
        {
            while (descriptor->Name != 0)
            {
                var importLibraryName      = new string((sbyte*)((long)imageBase + descriptor->Name));
                var cleanImportLibraryName = Path.GetFileName(importLibraryName);

                if (cleanImportLibraryName.Equals(libraryName, StringComparison.OrdinalIgnoreCase))
                {
                    result = descriptor;
                    return true;
                }

                descriptor++;
            }

            result = descriptor;
            return false;
        }

    }
}
