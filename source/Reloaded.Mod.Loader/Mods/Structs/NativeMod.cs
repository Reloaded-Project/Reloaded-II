using System.Text;

namespace Reloaded.Mod.Loader.Mods.Structs;

/// <summary>
/// A wrapper for Reloaded's <see cref="IMod"/> interface for a native DLL.
/// </summary>
public class NativeMod : IModV1
{
    /// <summary>
    /// Handle to the native module.
    /// </summary>
    private IntPtr _moduleHandle;

    private ReloadedStart _start;
    private ReloadedStartEx _startEx;
    private ReloadedSuspend _reloadedSuspend;
    private ReloadedResume _reloadedResume;
    private ReloadedUnload _reloadedUnload;
    private ReloadedCanSuspend _reloadedCanSuspend;
    private ReloadedCanUnload _reloadedCanUnload;
    private InitializeASI _initializeAsi;
    private Init _init;
    private bool _started;
    private string _modDirectory;
    private string _userConfigDirectory;

    /// <summary>
    /// Creates an IMod wrapper for a native DLL.
    /// </summary>
    /// <param name="path">Path to the native DLL.</param>
    /// <param name="userConfigDirectory">Path to the directory where the mod's user configuration is stored, passed to mods exporting ReloadedStartEx.</param>
    public NativeMod(string path, string userConfigDirectory = null)
    {
        _modDirectory = Path.GetDirectoryName(Path.GetFullPath(path))!;
        _userConfigDirectory = userConfigDirectory;

        // Set new DLL Directory, load library and restore.
        // This could probably be better optimised but isn't a hot path, would rather save on memory, so it's no big deal.
        var builder = new StringBuilder(4096); // ought to be enough characters given most programs break at 260 anyway. 
        GetDllDirectoryW(builder.Length, builder);
        SetDllDirectoryW(Path.GetDirectoryName(path));
        _moduleHandle = LoadLibraryW(path);
        SetDllDirectoryW(builder.ToString());

        _start = GetDelegateForNativeFunction<ReloadedStart>(_moduleHandle, nameof(ReloadedStart));
        _startEx = GetDelegateForNativeFunction<ReloadedStartEx>(_moduleHandle, nameof(ReloadedStartEx));
        _reloadedSuspend = GetDelegateForNativeFunction<ReloadedSuspend>(_moduleHandle, nameof(ReloadedSuspend));
        _reloadedResume = GetDelegateForNativeFunction<ReloadedResume>(_moduleHandle, nameof(ReloadedResume));
        _reloadedUnload = GetDelegateForNativeFunction<ReloadedUnload>(_moduleHandle, nameof(ReloadedUnload));
        _reloadedCanSuspend = GetDelegateForNativeFunction<ReloadedCanSuspend>(_moduleHandle, nameof(ReloadedCanSuspend));
        _reloadedCanUnload = GetDelegateForNativeFunction<ReloadedCanUnload>(_moduleHandle, nameof(ReloadedCanUnload));
        _initializeAsi = GetDelegateForNativeFunction<InitializeASI>(_moduleHandle, nameof(InitializeASI));
        _init = GetDelegateForNativeFunction<Init>(_moduleHandle, nameof(Init));
    }

    // Note for implementation: There is no guarantee mod exports any function (Start, CanUnload, etc.). Start function might just be DllMain.

    public void Start(IModLoaderV1 loader)
    {
        // Try Reloaded Entry point and then others.
        if (_startEx != null)
        {
            // Extended entry point hands the mod its folders, so it can find its configuration.
            if (_userConfigDirectory != null)
                Directory.CreateDirectory(_userConfigDirectory);

            _startEx.Invoke(_modDirectory, _userConfigDirectory);
            _started = true;
        }
        else if (_start != null)
        {
            _start.Invoke();
            _started = true;
        }
        else if (_initializeAsi != null && !_started)
        {
            _initializeAsi.Invoke();
            _started = true;
        }
        else if (_init != null && !_started)
        {
            _init.Invoke();
            _started = true;
        }
    }

    public void Suspend() => _reloadedSuspend?.Invoke();
    public void Resume() => _reloadedResume?.Invoke();
    public void Unload() => _reloadedUnload?.Invoke();
    public bool CanUnload() => _reloadedCanUnload?.Invoke() ?? false;
    public bool CanSuspend() => _reloadedCanSuspend?.Invoke() ?? false;

    public Action Disposing { get; }

    // Utility Functions.
    private TDelegate GetDelegateForNativeFunction<TDelegate>(IntPtr moduleHandle, string functionName) where TDelegate : Delegate
    {
        var address = GetProcAddress(moduleHandle, functionName);
        return address != IntPtr.Zero ? Marshal.GetDelegateForFunctionPointer<TDelegate>(address) : null;
    }

    // Delegates for native Other Exports.
    private delegate void InitializeASI();
    private delegate void Init();

    // Delegates for native Reloaded Exports.
    private delegate void ReloadedStart();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
    private delegate void ReloadedStartEx(string modDirectory, string userConfigDirectory);

    private delegate void ReloadedSuspend();
    private delegate void ReloadedResume();
    private delegate void ReloadedUnload();
    private delegate bool ReloadedCanUnload();
    private delegate bool ReloadedCanSuspend();

    #region Native Imports
    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    public static extern IntPtr LoadLibraryW(string lpFileName);

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Ansi)]
    public static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);
    
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern int GetDllDirectoryW(int nBufferLength, StringBuilder lpPathName);
    
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern bool SetDllDirectoryW(string lpPathName);
    #endregion
}