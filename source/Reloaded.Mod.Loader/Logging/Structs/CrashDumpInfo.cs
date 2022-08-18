namespace Reloaded.Mod.Loader.Logging.Structs;

/// <summary>
/// Contains information related to a specific crash.
/// </summary>
public class CrashDumpInfo
{
    /// <summary>
    /// List of enabled mods (for the application) at the time of the crash.
    /// </summary>
    public string[] EnabledMods { get; set; }

    /// <summary>
    /// List of running mods at the time of crash.
    /// </summary>
    public List<EnabledModInfo> RunningMods { get; set; } = new();

    /// <summary>
    /// Address where the crash has occurred, in hexadecimal.
    /// </summary>
    public string CrashAddress { get; set; }

    /// <summary>
    /// Path to the DLL/EXE where the crash has occurred, if available.
    /// </summary>
    public string FaultingModulePath { get; set; } = string.Empty;

    /// <summary>
    /// For serialization.
    /// </summary>
    public CrashDumpInfo() { }

    /// <summary>
    /// Creates a crash info.
    /// </summary>
    /// <param name="loader">The loader instance.</param>
    /// <param name="exceptionPointers">Exception pointers.</param>
    public unsafe CrashDumpInfo(Loader loader, Kernel32.EXCEPTION_POINTERS* exceptionPointers)
    {
        EnabledMods = loader.Application.EnabledMods;
        foreach (var loadedMod in loader.GetLoadedModInfo())
            RunningMods.Add(new EnabledModInfo(loadedMod));

        var crashAddress = exceptionPointers->ExceptionRecord->ExceptionAddress;
        CrashAddress = crashAddress.ToString("X");
        foreach (ProcessModule module in Process.GetCurrentProcess().Modules)
        {
            var startAddress = (nuint)(nint)module.BaseAddress;
            var endAddress   = (nuint)(nint)module.BaseAddress + (nuint)module.ModuleMemorySize;
            if (crashAddress >= startAddress && crashAddress < endAddress)
            {
                FaultingModulePath = module.FileName;
                break;
            }
        }
    }
}

/// <summary>
/// Info of the currently enabled mod.
/// </summary>
public class EnabledModInfo
{
    /// <summary>
    /// Identifier for this mod.
    /// </summary>
    public string ModId { get; set; }

    /// <summary>
    /// Current state of the mod.
    /// </summary>
    public ModState State { get; set; }

    /// <summary>
    /// Mod Version.
    /// </summary>
    public string Version { get; set; }

    /// <summary>
    /// For serialization.
    /// </summary>
    public EnabledModInfo() { }

    public EnabledModInfo(ModInfo mod)
    {
        ModId = mod.ModId;
        State = mod.State;
        Version = mod.Config.ModVersion;
    }
}