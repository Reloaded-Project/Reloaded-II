using System.Diagnostics;
using System.IO;
using System.Linq;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Loader.Logging;

namespace Reloaded.Mod.Loader.Utilities
{
    /// <summary>
    /// Scans for known DRMs and prints appropriate warnings.
    /// </summary>
    public static class DRMScanner
    {
        private const string SteamBindSection = ".bind";

        public static void PrintWarnings(Console console)
        {
            if (HasSteamStub())
                console.WriteLine("Warning: Steam Stub (Embedded Steam DRM) found.\n" +
                                  "This means EXE is encrypted at launch, which may render many mods unusable.\n" +
                                  "It is recommended that you either remove the DRM using `Steamless` or launch Reloaded II via another mod loader " +
                                  "that can handle Steam DRM encryption such as Ultimate ASI Loader. https://github.com/Reloaded-Project/Reloaded-II/blob/master/Docs/InjectionMethods.md#synchronous-and-asynchronous \n" +
                                  "Note: If you are already launching through any of these methods, you may ignore this message.", console.ColorYellowLight);
        }

        /// <summary>
        /// Returns true if Steam Stub DRM was found.
        /// </summary>
        public static unsafe bool HasSteamStub()
        {
            var thisProcess = Process.GetCurrentProcess();
            using (var stream = new UnmanagedMemoryStream((byte*) thisProcess.MainModule.BaseAddress, thisProcess.MainModule.ModuleMemorySize))
            {
                var peFile = new BasicPeParser(stream);
                return peFile.ImageSectionHeaders.Any(x => x.Name.ToString() == SteamBindSection);
            }
        }
    }
}
