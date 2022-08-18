using Reloaded.Hooks.Definitions.Structs;

namespace Reloaded.Mod.Loader.Utilities;

/// <summary>
/// Hook that handles dump generation on process crashing.
/// </summary>
public unsafe class ProcessCrashHook
{
    private static IHook<UnhandledExceptionFilterFuncPtr> _unhandledExceptionFilterHook;
    private static delegate*<IntPtr, int> _handleCrash;
    private static bool _initialized;

    public ProcessCrashHook(delegate*<IntPtr, int> handler)
    {
        if (_initialized)
            return;

        _handleCrash = handler;
        var kernel32 = Kernel32.GetModuleHandle("kernel32.dll");
        var unhandledAddr = Kernel32.GetProcAddress(kernel32, "UnhandledExceptionFilter");
        if (unhandledAddr != IntPtr.Zero)
            _unhandledExceptionFilterHook = ReloadedHooks.Instance.CreateHook<UnhandledExceptionFilterFuncPtr>(typeof(ProcessCrashHook), nameof(CrashHandlerImpl), (long)unhandledAddr).Activate();
    }

    [UnmanagedCallersOnly(CallConvs = new []{ typeof(CallConvStdcall) })]
    private static int CrashHandlerImpl(IntPtr exceptionPointers)
    {
        _handleCrash(exceptionPointers);
        return _unhandledExceptionFilterHook.OriginalFunction.Value.Invoke(exceptionPointers);
    }
    
    [Hooks.Definitions.X64.Function(Hooks.Definitions.X64.CallingConventions.Microsoft)]
    [Hooks.Definitions.X86.Function(Hooks.Definitions.X86.CallingConventions.Stdcall)]
    public struct UnhandledExceptionFilterFuncPtr { public FuncPtr<IntPtr, int> Value; }
}