using System.Runtime.InteropServices;

namespace Reloaded.Mod.Loader.IPC;

/// <summary>
/// [INTERNAL USE ONLY] Used for passing information without a server between loader, launcher and bootstrapper.
/// </summary>
[SupportedOSPlatform("windows")]
public class ReloadedMappedFile : IDisposable
{
    // DO NOT CHANGE. C++ BOOTSTRAPPER RELIES ON THIS NAME TOO!
    public static string GetMappedFileNameForPid(int pid) => $"Reloaded-Mod-Loader-Server-PID-{pid}";

    /// <summary>
    /// Modeled after the typical page size on windows.
    /// Any allocation smaller would allocate at least this size anyway.
    /// </summary>
    public const int AllocationSize = 4096;

    private MemoryMappedFile _memoryMappedFile;
    
    /// <summary>
    /// Creates or opens a memory mapped file that stores the Reloaded Mod Loader state for a given process ID.
    /// </summary>
    /// <param name="processId">The process ID to open mapped file for.</param>
    public ReloadedMappedFile(int processId)
    {
        _memoryMappedFile = MemoryMappedFile.CreateOrOpen(GetMappedFileNameForPid(processId), AllocationSize);
    }

    /// <summary>
    /// Returns true if a mapped file containing Reloaded state exists within the process.
    /// </summary>
    /// <param name="processId">The process to check.</param>
    /// <returns>True if exists, else false.</returns>
    public static bool Exists(int processId)
    {
        try
        {
            MemoryMappedFile.OpenExisting(GetMappedFileNameForPid(processId));
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>
    /// Gets the current state of the mod loader.
    /// </summary>
    public ReloadedLoaderState GetState()
    {
        using var accessor = _memoryMappedFile.CreateViewAccessor();
        accessor.Read(0, out ReloadedLoaderState state);
        return state;
    }

    /// <summary>
    /// Sets the port for the LiteNetLib server.
    /// </summary>
    /// <param name="port">The port to embed into the state.</param>
    public void SetPort(int port)
    {
        using var accessor = _memoryMappedFile.CreateViewAccessor();
        accessor.Write(ReloadedLoaderState.PortOffset, port);
    }

    /// <summary>
    /// Sets the flag for the mod loader being initialized.
    /// </summary>
    /// <param name="isInitialized">If Reloaded has successfully finished loading.</param>
    public void SetInitialized(bool isInitialized)
    {
        using var accessor = _memoryMappedFile.CreateViewAccessor();
        accessor.Write(ReloadedLoaderState.IsInitializedOffset, isInitialized);
    }

    /// <summary>
    /// Disposes this class instance.
    /// </summary>
    public void Dispose() => _memoryMappedFile.Dispose();
}

[StructLayout(LayoutKind.Explicit, Size = ReloadedMappedFile.AllocationSize)]
public struct ReloadedLoaderState
{
    public const int PortOffset = 0;
    public const int IsInitializedOffset = PortOffset + sizeof(int);

    /// <summary>
    /// Stores the port used to connect to the (LiteNetLib) Reloaded Mod Loader Server.
    /// </summary>
    [FieldOffset(PortOffset)]
    public int Port;

    /// <summary>
    /// True if the loader has finished initializing, else false.
    /// </summary>
    [FieldOffset(IsInitializedOffset)]
    public bool IsInitialized;
}