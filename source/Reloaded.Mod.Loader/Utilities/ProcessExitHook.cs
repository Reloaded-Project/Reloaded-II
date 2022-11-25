namespace Reloaded.Mod.Loader.Utilities;

/// <summary>
/// Utility which allows you to execute a given piece of code upon process exit.
/// </summary>
public class ProcessExitHook
{
    /// <summary>
    /// Executed when the process is about to exit.
    /// </summary>
    public event Action OnProcessExit;

    private static IHook<ExitProcess> _exitProcessHook;

    public ProcessExitHook(Action codeToRun, IReloadedHooks hooks) : this(hooks) => OnProcessExit += codeToRun;
    public ProcessExitHook(IReloadedHooks hooks)
    {
        // Hook native import for ExitProcess. (So we can save log on exit)
        var kernel32 = Kernel32.GetModuleHandle("kernel32.dll");
        var address = Kernel32.GetProcAddress(kernel32, "ExitProcess");
        if (address != IntPtr.Zero)
            _exitProcessHook = hooks.CreateHook<ExitProcess>(ExitProcessImpl, address).Activate();
    }

    private void ExitProcessImpl(uint uexitcode)
    {
        OnProcessExit?.Invoke();
        _exitProcessHook.OriginalFunction(uexitcode);
    }

    [Hooks.Definitions.X64.Function(Hooks.Definitions.X64.CallingConventions.Microsoft)]
    [Hooks.Definitions.X86.Function(Hooks.Definitions.X86.CallingConventions.Cdecl)]
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void ExitProcess(uint uExitCode);
}