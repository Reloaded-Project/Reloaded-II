namespace Reloaded.Mod.Launcher.Lib.Utility;

/// <summary>
/// Class that can be used to inject Reloaded into an active process.
/// </summary>
public class ApplicationInjector
{
    private readonly int _modLoaderSetupTimeout;
    private readonly int _modLoaderSetupSleepTime;

    private Process _process;
    private BasicDllInjector _injector;

    /// <summary/>
    public ApplicationInjector(Process process)
    {
        _process  = process;
        _injector = new BasicDllInjector(process);

        var loaderConfig = IoC.Get<LoaderConfig>();
        _modLoaderSetupTimeout   = loaderConfig.LoaderSetupTimeout;
        _modLoaderSetupSleepTime = loaderConfig.LoaderSetupSleeptime;
    }

    /// <summary>
    /// Injects the Reloaded bootstrapper into an active process.
    /// </summary>
    /// <exception cref="ArgumentException">DLL Injection failed, likely due to bad DLL or application.</exception>
    public void Inject()
    {
        long handle = _injector.Inject(GetBootstrapperPath(_process));
        if (handle == 0)
            throw new ArgumentException(Resources.ErrorDllInjectionFailed.Get());

        try
        {
            // Wait until mod loader loads.
            // If debugging, ignore timeout.
            bool WhileCondition()
            {
                if (CheckRemoteDebuggerPresent(_process.Handle, out var isDebuggerPresent))
                    return isDebuggerPresent;

                return false;
            }

            ActionWrappers.TryGetValueWhile(() =>
            {
                // Exit if application crashes while loading Reloaded.
                if (_process.HasExited)
                    return 0;

                if (!ReloadedMappedFile.Exists(_process.Id))
                    throw new Exception("Reloaded isn't yet loaded.");

                using var file = new ReloadedMappedFile(_process.Id);
                if (!file.GetState().IsInitialized)
                    throw new Exception("Reloaded is loaded but not fully initalized.");

                return 0;
            }, WhileCondition, _modLoaderSetupTimeout, _modLoaderSetupSleepTime);
        }
        catch (Exception e)
        {
            ActionWrappers.ExecuteWithApplicationDispatcher(() =>
            {
                Errors.HandleException(new Exception(Resources.ErrorFailedToObtainPort.Get(), e));
            });
        };
    }

    private string GetBootstrapperPath(Process process)
    {
        var config = IoC.Get<LoaderConfig>();
        return process.Is64Bit() ? config.Bootstrapper64Path : config.Bootstrapper32Path;
    }

    /* Native Imports */
    [DllImport("Kernel32.dll", SetLastError = true, ExactSpelling = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool CheckRemoteDebuggerPresent(IntPtr hProcess, [MarshalAs(UnmanagedType.Bool)] out bool isDebuggerPresent);
}