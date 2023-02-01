#pragma warning disable CS1591

namespace Reloaded.Mod.Loader.Server;

public static class ServerUtility
{
    /// <summary>
    /// Attempts to acquire the port to connect to a remote process with a specific id.
    /// </summary>
    /// <param name="pid">Process ID of the process to check.</param>
    /// <exception cref="FileNotFoundException"><see cref="MemoryMappedFile"/> was not created by the mod loader, in other words, mod loader is not loaded.</exception>
    /// <returns>0 if Reloaded is still initializing, exception if not initialized, else a valid port.</returns>
    [SupportedOSPlatform("windows")]
    public static int GetPort(int pid)
    {
        if (!ReloadedMappedFile.Exists(pid))
            throw new FileNotFoundException("Reloaded is not loaded into target process. No Memory Mapped File Exists.");

        using var mappedFile = new ReloadedMappedFile(pid);
        return mappedFile.GetState().Port;
    }

    /// <summary>
    /// Writes a server port for a specified process.
    /// </summary>
    /// <param name="processId">ID of the process to make a memory mapped file for.</param>
    /// <param name="port">The port used by the server.</param>
    [SupportedOSPlatform("windows")]
    public static void WriteServerPort(int processId, int port)
    {
        using var file = new ReloadedMappedFile(processId);
        file.SetPort(port);
    }
}