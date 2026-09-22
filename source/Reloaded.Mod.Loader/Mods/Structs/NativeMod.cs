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
    private nint _moduleHandle;

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
    private string _modId;
    private nint _loaderApiTable;

    /// <summary>
    /// Creates an IMod wrapper for a native DLL.
    /// </summary>
    /// <param name="path">Path to the native DLL.</param>
    /// <param name="userConfigDirectory">Path to the directory where the mod's user configuration is stored, passed to mods exporting ReloadedStartEx.</param>
    /// <param name="loaderApiTable">Pointer to the native loader API table shared by all mods, passed to mods exporting ReloadedStartEx.</param>
    /// <param name="modId">Id of this mod, handed to the mod with the loader API.</param>
    public NativeMod(string path, string userConfigDirectory = null, nint loaderApiTable = default, string modId = null)
    {
        _modDirectory = Path.GetDirectoryName(Path.GetFullPath(path))!;
        _userConfigDirectory = userConfigDirectory;
        _modId = modId ?? string.Empty;
        _loaderApiTable = loaderApiTable;

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

            InvokeStartEx();
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

    /// <summary>
    /// Call the ReloadedStartEx export, passing the mod its directories and the
    /// loader API through a versioned struct.
    /// </summary>
    private void InvokeStartEx()
    {
        var info = new NativeReloadedStartInfo()
        {
            ApiVersion = 1,
            ModDirectory = Marshal.StringToHGlobalUni(_modDirectory),
            UserConfigDirectory = Marshal.StringToHGlobalUni(_userConfigDirectory),
            ModId = StringToHGlobalUTF8(_modId),
            LoaderApi = _loaderApiTable
        };

        try
        {
            _startEx.Invoke(ref info);
        }
        finally
        {
            if (info.ModDirectory != nint.Zero)
                Marshal.FreeHGlobal(info.ModDirectory);

            if (info.UserConfigDirectory != nint.Zero)
                Marshal.FreeHGlobal(info.UserConfigDirectory);

            if (info.ModId != nint.Zero)
                Marshal.FreeHGlobal(info.ModId);
        }
    }

    // Utility Functions.
    private TDelegate GetDelegateForNativeFunction<TDelegate>(nint moduleHandle, string functionName) where TDelegate : Delegate
    {
        var address = GetProcAddress(moduleHandle, functionName);
        return address != nint.Zero ? Marshal.GetDelegateForFunctionPointer<TDelegate>(address) : null;
    }

    /// <summary>
    /// Copies a string to unmanaged memory as UTF-8; free with <see cref="Marshal.FreeHGlobal"/>.
    /// </summary>
    private static nint StringToHGlobalUTF8(string value)
    {
        var bytes = Encoding.UTF8.GetBytes(value);
        var pointer = Marshal.AllocHGlobal(bytes.Length + 1);
        Marshal.Copy(bytes, 0, pointer, bytes.Length);
        Marshal.WriteByte(pointer, bytes.Length, 0);
        return pointer;
    }

    // Delegates for native Other Exports.
    private delegate void InitializeASI();
    private delegate void Init();

    // Delegates for native Reloaded Exports.
    private delegate void ReloadedStart();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void ReloadedStartEx(ref NativeReloadedStartInfo info);

    private delegate void ReloadedSuspend();
    private delegate void ReloadedResume();
    private delegate void ReloadedUnload();
    private delegate bool ReloadedCanUnload();
    private delegate bool ReloadedCanSuspend();

    /// <summary>
    /// Information handed to native mods exporting ReloadedStartEx.
    /// New fields are only valid when <see cref="ApiVersion"/> is high enough,
    /// (this is to make sure the struct stays a stable contract).
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct NativeReloadedStartInfo
    {
        /// <summary>
        /// Version of the struct, starts at 1. 
        /// </summary>
        public int ApiVersion;

        /// <summary>
        /// Folder with the mod's own files (ConfigSchema.json, ...).
        /// UTF-16 string, only valid for the duration of the call.
        /// </summary>
        public nint ModDirectory;

        /// <summary>
        /// Folder where the launcher stores the user settings.
        /// UTF-16 string, only valid for the duration of the call.
        /// </summary>
        public nint UserConfigDirectory;

        /// <summary>
        /// Id of the mod being started.
        /// UTF-8 string, only valid for the duration of the call.
        /// </summary>
        public nint ModId;

        /// <summary>
        /// Wrapper around the loader API (<see cref="IModLoader"/>), usable to load,
        /// unload and query other mods. Stays valid past the call,
        /// for the lifetime of the mod.
        /// </summary>
        public nint LoaderApi;
    }

    #region Native Imports
    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    public static extern nint LoadLibraryW(string lpFileName);

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Ansi)]
    public static extern nint GetProcAddress(nint hModule, string lpProcName);
    
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern int GetDllDirectoryW(int nBufferLength, StringBuilder lpPathName);
    
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern bool SetDllDirectoryW(string lpPathName);
    #endregion
}