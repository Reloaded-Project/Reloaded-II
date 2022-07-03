#pragma warning disable CS1591
namespace Reloaded.Mod.Loader.Server;

public static class ServerUtility
{
    // DO NOT CHANGE. C++ BOOTSTRAPPER RELIES ON THIS NAME TOO!
    public static string GetMappedFileNameForPid(int pid) => $"Reloaded-Mod-Loader-Server-PID-{pid}";
}