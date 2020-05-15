using System.Diagnostics;
using System.IO;
using System.Linq;

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
            using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            return HasSteamStub(fileStream);
        }

        /// <summary>
        /// Returns true if Steam Stub DRM was found.
        /// </summary>
        public static unsafe bool HasSteamStub(Stream stream)
        {
            using var parser = new BasicPeParser(stream);
            return parser.ImageSectionHeaders.Any(x => x.Name.ToString() == SteamBindSection);
        }

        /// <summary>
        /// Returns true if Steam Stub DRM is in the current process.
        /// </summary>
        public static unsafe bool CheckCurrentProcess()
        {
            var process = Process.GetCurrentProcess();
            using var stream = new UnmanagedMemoryStream((byte*)process.MainModule.BaseAddress, process.MainModule.ModuleMemorySize);
            return HasSteamStub(stream);
        }
    }
}
