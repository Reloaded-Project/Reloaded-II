namespace Reloaded.Mod.Loader.Mods.Structs;

/// <summary>
/// Layout of the loader API table handed for native mods, it must stay in sync
/// with <c>ReloadedLoaderApi</c> in ReloadedModConfig.h.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct NativeReloadedLoaderApiTable
{
    public int ApiVersion;

    public nint LoadMod;
    public nint UnloadMod;
    public nint SuspendMod;
    public nint ResumeMod;
    public nint GetDirectoryForMod;
    public nint GetModConfigDirectory;
    public nint Write;
    public nint WriteAsync;
    public nint WriteLine;
    public nint WriteLineAsync;
    public nint FreeString;
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
    public delegate void Utf8Action(nint valueUtf8);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate nint Utf8ToString(nint valueUtf8);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void FreeAction(nint value);

    private readonly IModLoader _loader;
    private readonly Logger _logger;
    private nint _tablePointer;


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
    public nint TablePointer => _tablePointer;

    private void LoadMod(nint modIdUtf8)
    {
        try { _loader.LoadMod(ReadUtf8(modIdUtf8)); }
        catch (Exception e) { LogError(e, nameof(LoadMod)); }
    }

    private void UnloadMod(nint modIdUtf8)
    {
        try { _loader.UnloadMod(ReadUtf8(modIdUtf8)); }
        catch (Exception e) { LogError(e, nameof(UnloadMod)); }
    }

    private void SuspendMod(nint modIdUtf8)
    {
        try { _loader.SuspendMod(ReadUtf8(modIdUtf8)); }
        catch (Exception e) { LogError(e, nameof(SuspendMod)); }
    }

    private void ResumeMod(nint modIdUtf8)
    {
        try { _loader.ResumeMod(ReadUtf8(modIdUtf8)); }
        catch (Exception e) { LogError(e, nameof(ResumeMod)); }
    }

    private nint GetDirectoryForMod(nint modIdUtf8)
    {
        try { return Marshal.StringToHGlobalUni(_loader.GetDirectoryForModId(ReadUtf8(modIdUtf8))); }
        catch (Exception e) { LogError(e, nameof(GetDirectoryForMod)); return nint.Zero; }
    }

    private nint GetModConfigDirectory(nint modIdUtf8)
    {
        try { return Marshal.StringToHGlobalUni(_loader.GetModConfigDirectory(ReadUtf8(modIdUtf8))); }
        catch (Exception e) { LogError(e, nameof(GetModConfigDirectory)); return nint.Zero; }
    }

    private void Write(nint textUtf8)
    {
        try { _logger?.Write(ReadUtf8(textUtf8)); }
        catch (Exception e) { LogError(e, nameof(Write)); }
    }

    private void WriteAsync(nint textUtf8)
    {
        try { _logger?.WriteAsync(ReadUtf8(textUtf8)); }
        catch (Exception e) { LogError(e, nameof(WriteAsync)); }
    }

    private void WriteLine(nint textUtf8)
    {
        try { _logger?.WriteLine(ReadUtf8(textUtf8)); }
        catch (Exception e) { LogError(e, nameof(WriteLine)); }
    }

    private void WriteLineAsync(nint textUtf8)
    {
        try { _logger?.WriteLineAsync(ReadUtf8(textUtf8)); }
        catch (Exception e) { LogError(e, nameof(WriteLineAsync)); }
    }

    /// <summary>
    /// Gives back a string handed out by the functions above. Mods can't free it
    /// themselves, the memory comes from our side of the fence, not their CRT.
    /// </summary>
    private void FreeString(nint value)
    {
        try
        {
            if (value != nint.Zero)
                Marshal.FreeHGlobal(value);
        }
        catch (Exception e) { LogError(e, nameof(FreeString)); }
    }

    private void LogError(Exception e, string function) => _logger?.WriteLineAsync($"[NativeLoaderApi] {function} failed: {e.Message}");

    private static string ReadUtf8(nint pointer) => pointer == nint.Zero ? string.Empty : Marshal.PtrToStringUTF8(pointer)!;

    public void Dispose()
    {
        if (_tablePointer == nint.Zero)
            return;

        Marshal.FreeHGlobal(_tablePointer);
        _tablePointer = nint.Zero;
    }
}
