using System.IO;

namespace Reloaded.Mod.Loader.Utilities.Steam
{
    public static class SteamAppId
    {
        public const string FileName = "steam_appid.txt";
        
        public static bool Exists(string directory) => File.Exists(Path.Combine(directory, FileName));
        public static void WriteToDirectory(string directory, int appId) => File.WriteAllText(Path.Combine(directory, FileName), appId.ToString());
    }
}
