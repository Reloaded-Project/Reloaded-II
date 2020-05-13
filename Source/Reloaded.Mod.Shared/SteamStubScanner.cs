using System.Diagnostics;
using System.IO;
using System.Linq;
using PeNet;

namespace Reloaded.Mod.Shared
{
    public static class SteamStubScanner
    {
        private const string SteamBindSection = ".bind";

        /// <summary>
        /// Returns true if Steam Stub DRM was found.
        /// </summary>
        public static unsafe bool HasSteamStub(string filePath)
        {
            var peFile = new PeFile(filePath);
            return peFile.ImageSectionHeaders.Any(x => x.Name.ToString() == SteamBindSection);
        }

        /// <summary>
        /// Returns true if Steam Stub DRM was found.
        /// </summary>
        public static unsafe bool HasSteamStub(Stream stream)
        {
            var peFile = new PeFile(stream);
            return peFile.ImageSectionHeaders.Any(x => x.Name.ToString() == SteamBindSection);
        }

        /// <summary>
        /// Returns true if Steam Stub DRM is in the current process.
        /// </summary>
        public static unsafe bool CheckCurrentProcess()
        {
            var process = Process.GetCurrentProcess();
            using (var stream = new UnmanagedMemoryStream((byte*)process.MainModule.BaseAddress, process.MainModule.ModuleMemorySize))
            {
                return HasSteamStub(stream);
            }
        }
    }
}
