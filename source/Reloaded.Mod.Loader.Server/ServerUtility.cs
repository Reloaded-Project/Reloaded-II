#pragma warning disable CS1591
using System.IO.MemoryMappedFiles;
using System.Runtime.Versioning;

namespace Reloaded.Mod.Loader.Server;

public static class ServerUtility
{
    // DO NOT CHANGE. C++ BOOTSTRAPPER RELIES ON THIS NAME TOO!
    public static string GetMappedFileNameForPid(int pid) => $"Reloaded-Mod-Loader-Server-PID-{pid}"; // LiteNetLib Specific

    /// <summary>
    /// Returns true if the mod loader is loaded and present inside a specified process. Else returns false.
    /// </summary>
    /// <param name="process">The process to check for.</param>
    [SupportedOSPlatform("windows")]
    public static bool IsServerPresent(Process process)
    {
        try
        {
            GetPort(process.Id);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>
    /// Attempts to acquire the port to connect to a remote process with a specific id.
    /// </summary>
    /// <exception cref="FileNotFoundException"><see cref="MemoryMappedFile"/> was not created by the mod loader, in other words, mod loader is not loaded.</exception>
    /// <returns>0 if Reloaded is still initializing, exception if not initialized, else a valid port.</returns>
    [SupportedOSPlatform("windows")]
    public static int GetPort(int pid)
    {
        var mappedFile = MemoryMappedFile.OpenExisting(GetMappedFileNameForPid(pid));
        var view = mappedFile.CreateViewStream();
        var binaryReader = new BinaryReader(view);
        var port = binaryReader.ReadInt32();
        if (port < 0)
            throw new Exception("Invalid Port Number, Should be 0 - 65535");

        return port;
    }

    /// <summary>
    /// Writes a server port for a specified process.
    /// </summary>
    /// <param name="processId">ID of the process to make a memory mapped file for.</param>
    /// <param name="port">The port used by the server.</param>
    [SupportedOSPlatform("windows")]
    public static MemoryMappedFile WriteServerPort(int processId, int port)
    {   
        var result = MemoryMappedFile.CreateOrOpen(GetMappedFileNameForPid(processId), sizeof(int));
        WriteServerPort(result, port);
        return result;
    }

    /// <summary>
    /// Writes a server port into a memory mapped file.
    /// </summary>
    /// <param name="file">The memory mapped file to write port info to.</param>
    /// <param name="port">The port to write.</param>
    public static void WriteServerPort(MemoryMappedFile file, int port)
    {
        var view = file.CreateViewStream();
        var binaryWriter = new BinaryWriter(view);
        binaryWriter.Write(port);
    }
}