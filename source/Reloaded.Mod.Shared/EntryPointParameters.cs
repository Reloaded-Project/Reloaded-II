namespace Reloaded.Mod.Shared;

/// <summary>
/// Parameters used to pass properties from native code to managed code.
/// See: EntryPointParameters.h
/// </summary>
public struct EntryPointParameters
{
    /*
        NOTE: Change CurrentVersion when adding new fields or extending current fields.
              Also update EntryPointParameter.h in bootstrapper accordingly.
    */

    /// <summary>
    /// Current version of parameters.
    /// </summary>
    public const int CurrentVersion = 8;

    // Version 1
    /// <summary>
    /// Version of the Reloaded bootstrapper.
    /// </summary>
    public int Version;

    /// <summary>
    /// Contains various information about how Reloaded has been loaded.
    /// </summary>
    public EntryPointFlags Flags;

    // Version 2
    // ...

    /// <summary>
    /// Contains the path to the native DLL which loaded the loader.
    /// </summary>
    public unsafe char* DllPath;
    
    /// <summary>
    /// Checks if struct is using latest version.
    /// </summary>
    public bool IsLatestVersion() => Version == CurrentVersion;

    /// <summary>
    /// Copies data from the passed in native struct to a new struct.
    /// This allows for old bootstrappers to work with more recent parameters.
    /// </summary>
    /// <param name="pointer">Pointer passed in from native code.</param>
    public static unsafe EntryPointParameters Copy(EntryPointParameters* pointer)
    {
        // Copy whole if possible.
        if (pointer->IsLatestVersion())
            return *pointer;

        // Otherwise construct from available size.
        EntryPointParameters result = default;

        // Version 1
        if (pointer->Version >= 1)
        {
            result.Version = pointer->Version;
            result.Flags = pointer->Flags;
        }
        
        if (pointer->Version >= 8)
        {
            result.DllPath = pointer->DllPath;
        }

        return result;
    }
    
    /// <summary>
    /// True if reading the DLL path is supported.
    /// </summary>
    public bool SupportsDllPath => Version >= 8;
}

/// <summary>
/// See: EntryPointParameters.h
/// </summary>
[Flags]
public enum EntryPointFlags : int
{
    /// <summary>
    /// Nothing special.
    /// </summary>
    None = 0,
    
    /// <summary>
    /// Reloaded has been loaded via an external mod loader or DLL hijacking method.
    /// </summary>
    LoadedExternally = 1,
}