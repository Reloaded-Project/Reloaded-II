
using System.Diagnostics.CodeAnalysis;
#if NET8_0_OR_GREATER
using System.Runtime.InteropServices.Marshalling;
#endif

namespace Reloaded.Mod.Installer.Lib.Utilities;

#if NET8_0_OR_GREATER
[GeneratedComClass]
#else
[ComImport]
#endif
[Guid("00021401-0000-0000-C000-000000000046")]
internal partial class ShellLink
{
}

#if NET8_0_OR_GREATER
[GeneratedComInterface(StringMarshalling = StringMarshalling.Utf16)]
#else
[ComImport]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
#endif
[Guid("000214F9-0000-0000-C000-000000000046")]
internal partial interface IShellLink
{
    void GetPath(IntPtr pszFile, int cchMaxPath, IntPtr pfd, int fFlags);
    void GetIDList(out IntPtr ppidl);
    void SetIDList(IntPtr pidl);
    void GetDescription(IntPtr pszName, int cchMaxName);
    void SetDescription(string pszName);
    void GetWorkingDirectory(IntPtr pszDir, int cchMaxPath);
    void SetWorkingDirectory(string pszDir);
    void GetArguments(IntPtr pszArgs, int cchMaxPath);
    void SetArguments(string pszArgs);
    void GetHotkey(out short pwHotkey);
    void SetHotkey(short wHotkey);
    void GetShowCmd(out int piShowCmd);
    void SetShowCmd(int iShowCmd);
    void GetIconLocation(IntPtr pszIconPath, int cchIconPath, out int piIcon);
    void SetIconLocation(string pszIconPath, int iIcon);
    void SetRelativePath(string pszPathRel, int dwReserved);
    void Resolve(IntPtr hwnd, int fFlags);
    void SetPath(string pszFile);
}

#if NET8_0_OR_GREATER
[GeneratedComInterface(StringMarshalling = StringMarshalling.Utf16)]
#else
[ComImport]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
#endif
[Guid("0000010B-0000-0000-C000-000000000046")]
internal partial interface IPersistFile
{
    void GetCurFile(out IntPtr ppszFileName);
    void IsDirty();
    void Load(string pszFileName, uint dwMode);
    void Save(string pszFileName, [MarshalAs(UnmanagedType.I1)] bool fRemember);
    void SaveCompleted(string pszFileName);
}

internal static class NativeShellLink
{
    [DllImport("ole32.dll")]
    public static extern int CoCreateInstance(
        [MarshalAs(UnmanagedType.LPStruct)] Guid rclsid,
        IntPtr pUnkOuter,
        uint dwClsContext,
        [MarshalAs(UnmanagedType.LPStruct)] Guid riid,
        out IntPtr ppv);

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    private static IShellLink CreateShellLink(out nint pShellLink)
    {
        var CLSID_ShellLink = new Guid("00021401-0000-0000-C000-000000000046");
        var IID_IShellLink = new Guid("000214F9-0000-0000-C000-000000000046");
            
        const uint CLSCTX_INPROC_SERVER = 1;
        var hResult = CoCreateInstance(CLSID_ShellLink, IntPtr.Zero, CLSCTX_INPROC_SERVER, IID_IShellLink, out pShellLink);
        if (hResult != 0)
        {
            throw new COMException("Failed to create ShellLink instance", hResult);
        }

        
#if NET8_0_OR_GREATER
        ComWrappers cw = new StrategyBasedComWrappers();
        return (IShellLink)cw.GetOrCreateObjectForComInstance(pShellLink, CreateObjectFlags.None);
#else
return (IShellLink)Marshal.GetObjectForIUnknown(pShellLink);
        #endif
    }
    
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public static void MakeShortcut(string shortcutPath, string executablePath)
    {
        #if NET8_0_OR_GREATER
        var shell = CreateShellLink(out var pShellLink);
        #else
        var shell = (IShellLink)new ShellLink();
        #endif
        
        shell.SetDescription($"Reloaded II");
        shell.SetPath($"\"{executablePath}\"");
        shell.SetWorkingDirectory(Path.GetDirectoryName(executablePath)!);

        #if NET8_0_OR_GREATER 
        ComWrappers cw = new StrategyBasedComWrappers();
        var file = (IPersistFile)cw.GetOrCreateObjectForComInstance(pShellLink, CreateObjectFlags.None);
        file.Save(shortcutPath, false);
        #else
        var file = (IPersistFile)shell;
        file.Save(shortcutPath, false);
        #endif
    }
}