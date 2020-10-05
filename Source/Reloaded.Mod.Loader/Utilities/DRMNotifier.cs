using System.Diagnostics;
using Reloaded.Mod.Loader.Logging;
using Reloaded.Mod.Loader.Utilities.DRM;
using Reloaded.Mod.Shared;

namespace Reloaded.Mod.Loader.Utilities
{
    /// <summary>
    /// Scans for known DRMs and prints appropriate warnings.
    /// </summary>
    public static class DRMNotifier
    {
        public static void PrintWarnings(BasicPeParser parser, Console console)
        {
            if (SteamStubScanner.HasSteamStub(parser))
                console.WriteLine("Warning: Steam Stub (Embedded Steam DRM) found.\n" +
                                  "This means EXE is encrypted at launch, which may render many mods unusable.\n" +
                                  "It is recommended that you either remove the DRM using `Steamless` or launch Reloaded II via another mod loader " +
                                  "that can handle Steam DRM encryption such as Ultimate ASI Loader. https://github.com/Reloaded-Project/Reloaded-II/blob/master/Docs/InjectionMethods.md#synchronous-and-asynchronous \n" +
                                  "Note: If you are already launching through any of these methods, you may ignore this message.", console.ColorYellowLight);
        }
    }
}
