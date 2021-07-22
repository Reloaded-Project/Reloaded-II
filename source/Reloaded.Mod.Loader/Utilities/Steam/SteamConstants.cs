using System.Drawing;
using Reloaded.Mod.Interfaces;

namespace Reloaded.Mod.Loader.Utilities.Steam
{
    public static class SteamConstants
    {
        public static string Prefix = "R-II SteamHook";
        public static void SteamWriteLineAsync(this ILogger logger, string message, Color color) => logger.WriteLineAsync($"[{Prefix}] {message}", color);
    }
}
