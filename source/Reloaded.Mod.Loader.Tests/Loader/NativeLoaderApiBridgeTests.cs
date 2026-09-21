using System.Runtime.InteropServices;
using Reloaded.Mod.Interfaces;

namespace Reloaded.Mod.Loader.Tests.Loader;

/// <summary>
/// Tests the native function pointer table wrapping the loader API.
/// The calls go through the raw pointers, exactly like a native mod would.
/// </summary>
public class NativeLoaderApiBridgeTests : IDisposable
{
    private readonly Mock<IModLoader> _loader = new();
    private readonly NativeLoaderApiBridge _bridge;

    public NativeLoaderApiBridgeTests()
    {
        _bridge = new NativeLoaderApiBridge(_loader.Object);
    }

    [Fact]
    public void Table_Has_Version_And_Functions()
    {
        // Act
        var table = ReadTable();

        // Assert
        Assert.Equal(1, table.ApiVersion);
        Assert.NotEqual(IntPtr.Zero, table.LoadMod);
        Assert.NotEqual(IntPtr.Zero, table.GetModConfigDirectory);
        Assert.NotEqual(IntPtr.Zero, table.Log);
        Assert.NotEqual(IntPtr.Zero, table.FreeString);
        Assert.NotEqual(IntPtr.Zero, table.LogAsync);
    }

    [Fact]
    public void GetModConfigDirectory_Returns_The_String()
    {
        // Arrange
        _loader.Setup(l => l.GetModConfigDirectory("some.mod")).Returns(@"D:\User\Mods\SomeMod");
        var getString = Marshal.GetDelegateForFunctionPointer<NativeLoaderApiBridge.Utf8ToString>(ReadTable().GetModConfigDirectory);

        // Act
        var pointer = getString(ToUtf8("some.mod"));

        // Assert
        // We own the memory, so it goes back through the table's free function.
        Assert.Equal(@"D:\User\Mods\SomeMod", Marshal.PtrToStringUni(pointer));

        var freeString = Marshal.GetDelegateForFunctionPointer<NativeLoaderApiBridge.FreeAction>(ReadTable().FreeString);
        freeString(pointer);
    }

    [Fact]
    public void FreeString_Ignores_Null()
    {
        // Arrange
        // Native mods may hand back whatever the getters returned, zero included.
        var freeString = Marshal.GetDelegateForFunctionPointer<NativeLoaderApiBridge.FreeAction>(ReadTable().FreeString);

        // Act
        freeString(IntPtr.Zero);
    }

    [Fact]
    public void GetDirectoryForMod_Returns_Zero_Instead_Of_Throwing()
    {
        // Arrange
        // Unknown mods throw inside the loader; native callers must never see that.
        _loader.Setup(l => l.GetDirectoryForModId("nope.mod")).Throws(new KeyNotFoundException());
        var getString = Marshal.GetDelegateForFunctionPointer<NativeLoaderApiBridge.Utf8ToString>(ReadTable().GetDirectoryForMod);

        // Act
        var result = getString(ToUtf8("nope.mod"));

        // Assert
        Assert.Equal(IntPtr.Zero, result);
    }

    [Fact]
    public void ModStateFunctions_Forward_To_Loader()
    {
        // Arrange
        var loadMod = Marshal.GetDelegateForFunctionPointer<NativeLoaderApiBridge.Utf8Action>(ReadTable().LoadMod);
        var unloadMod = Marshal.GetDelegateForFunctionPointer<NativeLoaderApiBridge.Utf8Action>(ReadTable().UnloadMod);
        var suspendMod = Marshal.GetDelegateForFunctionPointer<NativeLoaderApiBridge.Utf8Action>(ReadTable().SuspendMod);
        var resumeMod = Marshal.GetDelegateForFunctionPointer<NativeLoaderApiBridge.Utf8Action>(ReadTable().ResumeMod);

        // Act
        loadMod(ToUtf8("some.mod"));
        unloadMod(ToUtf8("some.mod"));
        suspendMod(ToUtf8("some.mod"));
        resumeMod(ToUtf8("some.mod"));

        // Assert
        _loader.Verify(l => l.LoadMod("some.mod"), Times.Once);
        _loader.Verify(l => l.UnloadMod("some.mod"), Times.Once);
        _loader.Verify(l => l.SuspendMod("some.mod"), Times.Once);
        _loader.Verify(l => l.ResumeMod("some.mod"), Times.Once);
    }

    [Fact]
    public void Dispose_Releases_The_Table()
    {
        // Arrange
        var pointer = _bridge.TablePointer;

        // Act
        _bridge.Dispose();

        // Assert
        Assert.Equal(IntPtr.Zero, _bridge.TablePointer);
    }

    private NativeReloadedLoaderApiTable ReadTable() => Marshal.PtrToStructure<NativeReloadedLoaderApiTable>(_bridge.TablePointer);

    private static IntPtr ToUtf8(string value)
    {
        var bytes = Encoding.UTF8.GetBytes(value);
        var pointer = Marshal.AllocHGlobal(bytes.Length + 1);
        Marshal.Copy(bytes, 0, pointer, bytes.Length);
        Marshal.WriteByte(pointer, bytes.Length, 0);
        return pointer;
    }

    public void Dispose() => _bridge.Dispose();
}
