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

    public ProcessCrashHook(delegate*<IntPtr, int> handler, IReloadedHooks hooks)
    {
        if (_initialized)
            return;

        _initialized = true;
        _handleCrash = handler;
        var kernel32 = Kernel32.GetModuleHandle("kernel32.dll");
        var unhandledAddr = Kernel32.GetProcAddress(kernel32, "UnhandledExceptionFilter");
        if (unhandledAddr != IntPtr.Zero)
            _unhandledExceptionFilterHook = hooks.CreateHook<UnhandledExceptionFilterFuncPtr>(typeof(ProcessCrashHook), nameof(CrashHandlerImpl), (long)unhandledAddr).Activate();
    }

    [UnmanagedCallersOnly(CallConvs = new []{ typeof(CallConvStdcall) })]
    private static int CrashHandlerImpl(IntPtr exceptionPointers)
    {
        var result = _unhandledExceptionFilterHook.OriginalFunction.Value.Invoke(exceptionPointers);
        _handleCrash(exceptionPointers);
        return result;
    }

    [Hooks.Definitions.X64.Function(Hooks.Definitions.X64.CallingConventions.Microsoft)]
    [Hooks.Definitions.X86.Function(Hooks.Definitions.X86.CallingConventions.Stdcall)]
    public struct UnhandledExceptionFilterFuncPtr { public FuncPtr<IntPtr, int> Value; }
}