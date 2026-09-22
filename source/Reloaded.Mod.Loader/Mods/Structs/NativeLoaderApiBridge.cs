namespace Reloaded.Mod.Loader.Mods.Structs;

/// <summary>
/// Layout of the loader API table handed for native mods, it must stay in sync
/// with <c>ReloadedLoaderApi</c> in ReloadedModConfig.h.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct NativeReloadedLoaderApiTable
{
    public int ApiVersion;

    public IntPtr LoadMod;
    public IntPtr UnloadMod;
    public IntPtr SuspendMod;
    public IntPtr ResumeMod;
    public IntPtr GetDirectoryForMod;
    public IntPtr GetModConfigDirectory;
    public IntPtr Write;
    public IntPtr WriteAsync;
    public IntPtr WriteLine;
    public IntPtr WriteLineAsync;
    public IntPtr FreeString;
}

/// <summary>
/// Wraps the managed <see cref="IModLoader"/> into the native function pointer
/// table above. Shared by all mods.
/// </summary>
public sealed class NativeLoaderApiBridge : IDisposable
{
    private const int ApiVersion = 1;


    // Native mods build as cdecl, which is the MSVC default, so every pointer in
    // the table has to say so. Delegates default to stdcall instead, which would
    // wreck the stack on 32 bit games.
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void Utf8Action(IntPtr valueUtf8);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate IntPtr Utf8ToString(IntPtr valueUtf8);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void FreeAction(IntPtr value);

    private readonly IModLoader _loader;
    private readonly Logger _logger;
    private IntPtr _tablePointer;


    private readonly Utf8Action _loadMod;
    private readonly Utf8Action _unloadMod;
    private readonly Utf8Action _suspendMod;
    private readonly Utf8Action _resumeMod;
    private readonly Utf8ToString _getDirectoryForMod;
    private readonly Utf8ToString _getModConfigDirectory;
    private readonly Utf8Action _write;
    private readonly Utf8Action _writeAsync;
    private readonly Utf8Action _writeLine;
    private readonly Utf8Action _writeLineAsync;
    private readonly FreeAction _freeString;

    /// <summary>
    ///Wraps the loader and logger into a native API table.
    /// </summary>
    /// <param name="loader">The loader API given to mods.</param>
    /// <param name="logger">Logger writing to console and file; optional.</param>
    public NativeLoaderApiBridge(IModLoader loader, Logger logger = null)
    {
        _loader = loader;
        _logger = logger;

        _loadMod               = LoadMod;
        _unloadMod             = UnloadMod;
        _suspendMod            = SuspendMod;
        _resumeMod             = ResumeMod;
        _getDirectoryForMod    = GetDirectoryForMod;
        _getModConfigDirectory = GetModConfigDirectory;
        _write                 = Write;
        _writeAsync            = WriteAsync;
        _writeLine             = WriteLine;
        _writeLineAsync        = WriteLineAsync;
        _freeString            = FreeString;

        var table = new NativeReloadedLoaderApiTable()
        {
            ApiVersion            = ApiVersion,
            LoadMod               = Marshal.GetFunctionPointerForDelegate(_loadMod),
            UnloadMod             = Marshal.GetFunctionPointerForDelegate(_unloadMod),
            SuspendMod            = Marshal.GetFunctionPointerForDelegate(_suspendMod),
            ResumeMod             = Marshal.GetFunctionPointerForDelegate(_resumeMod),
            GetDirectoryForMod    = Marshal.GetFunctionPointerForDelegate(_getDirectoryForMod),
            GetModConfigDirectory = Marshal.GetFunctionPointerForDelegate(_getModConfigDirectory),
            Write                 = Marshal.GetFunctionPointerForDelegate(_write),
            WriteAsync            = Marshal.GetFunctionPointerForDelegate(_writeAsync),
            WriteLine             = Marshal.GetFunctionPointerForDelegate(_writeLine),
            WriteLineAsync        = Marshal.GetFunctionPointerForDelegate(_writeLineAsync),
            FreeString            = Marshal.GetFunctionPointerForDelegate(_freeString)
        };

        _tablePointer = Marshal.AllocHGlobal(Marshal.SizeOf<NativeReloadedLoaderApiTable>());
        Marshal.StructureToPtr(table, _tablePointer, fDeleteOld: false);
    }

    /// <summary>
    /// Pointer to the native table, placed inside ReloadedStartInfo
    /// </summary>
    public IntPtr TablePointer => _tablePointer;

    private void LoadMod(IntPtr modIdUtf8)
    {
        try { _loader.LoadMod(ReadUtf8(modIdUtf8)); }
        catch (Exception e) { LogError(e, nameof(LoadMod)); }
    }

    private void UnloadMod(IntPtr modIdUtf8)
    {
        try { _loader.UnloadMod(ReadUtf8(modIdUtf8)); }
        catch (Exception e) { LogError(e, nameof(UnloadMod)); }
    }

    private void SuspendMod(IntPtr modIdUtf8)
    {
        try { _loader.SuspendMod(ReadUtf8(modIdUtf8)); }
        catch (Exception e) { LogError(e, nameof(SuspendMod)); }
    }

    private void ResumeMod(IntPtr modIdUtf8)
    {
        try { _loader.ResumeMod(ReadUtf8(modIdUtf8)); }
        catch (Exception e) { LogError(e, nameof(ResumeMod)); }
    }

    private IntPtr GetDirectoryForMod(IntPtr modIdUtf8)
    {
        try { return Marshal.StringToHGlobalUni(_loader.GetDirectoryForModId(ReadUtf8(modIdUtf8))); }
        catch (Exception e) { LogError(e, nameof(GetDirectoryForMod)); return IntPtr.Zero; }
    }

    private IntPtr GetModConfigDirectory(IntPtr modIdUtf8)
    {
        try { return Marshal.StringToHGlobalUni(_loader.GetModConfigDirectory(ReadUtf8(modIdUtf8))); }
        catch (Exception e) { LogError(e, nameof(GetModConfigDirectory)); return IntPtr.Zero; }
    }

    private void Write(IntPtr textUtf8)
    {
        try { _logger?.Write(ReadUtf8(textUtf8)); }
        catch (Exception e) { LogError(e, nameof(Write)); }
    }

    private void WriteAsync(IntPtr textUtf8)
    {
        try { _logger?.WriteAsync(ReadUtf8(textUtf8)); }
        catch (Exception e) { LogError(e, nameof(WriteAsync)); }
    }

    private void WriteLine(IntPtr textUtf8)
    {
        try { _logger?.WriteLine(ReadUtf8(textUtf8)); }
        catch (Exception e) { LogError(e, nameof(WriteLine)); }
    }

    private void WriteLineAsync(IntPtr textUtf8)
    {
        try { _logger?.WriteLineAsync(ReadUtf8(textUtf8)); }
        catch (Exception e) { LogError(e, nameof(WriteLineAsync)); }
    }

    /// <summary>
    /// Gives back a string handed out by the functions above. Mods can't free it
    /// themselves, the memory comes from our side of the fence, not their CRT.
    /// </summary>
    private void FreeString(IntPtr value)
    {
        try
        {
            if (value != IntPtr.Zero)
                Marshal.FreeHGlobal(value);
        }
        catch (Exception e) { LogError(e, nameof(FreeString)); }
    }

    private void LogError(Exception e, string function) => _logger?.WriteLineAsync($"[NativeLoaderApi] {function} failed: {e.Message}");

    private static string ReadUtf8(IntPtr pointer) => pointer == IntPtr.Zero ? string.Empty : Marshal.PtrToStringUTF8(pointer)!;

    public void Dispose()
    {
        if (_tablePointer == IntPtr.Zero)
            return;

        Marshal.FreeHGlobal(_tablePointer);
        _tablePointer = IntPtr.Zero;
    }
}
