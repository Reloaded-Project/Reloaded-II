using dll_syringe.Net.Sys;

namespace Reloaded.Mod.Launcher.Lib.Utility;

/// <summary>
/// Class that can be used to inject Reloaded into an active process.
/// </summary>
public unsafe class ApplicationInjector : IDisposable
{
    private readonly int _modLoaderSetupTimeout;
    private readonly int _modLoaderSetupSleepTime;

    private Process _process;
    private CSyringe* _syringe;

    /// <summary/>
    public ApplicationInjector(Process process)
    {
        _process  = process;
        _syringe = NativeMethods.syringe_for_suspended_process((uint)_process.Id);

        var loaderConfig = IoC.Get<LoaderConfig>();
        _modLoaderSetupTimeout   = loaderConfig.LoaderSetupTimeout;
        _modLoaderSetupSleepTime = loaderConfig.LoaderSetupSleeptime;
    }
    
    ~ApplicationInjector() => Dispose();

    /// <inheritdoc />
    public void Dispose()
    {
        NativeMethods.syringe_free(_syringe);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Injects the Reloaded bootstrapper into an active process.
    /// </summary>
    /// <exception cref="ArgumentException">DLL Injection failed, likely due to bad DLL or application.</exception>
    public void Inject()
    {
        // TODO: This is slow and wasteful, change this when we change encoding in injector.
        var bootstrapperPath = GetBootstrapperPath(_process);
        var bootstrapperPathBytes = Encoding.UTF8.GetBytes(bootstrapperPath);
        var bootstrapperPathWithNull = new byte[bootstrapperPathBytes.Length + 1];
        Array.Copy(bootstrapperPathBytes, bootstrapperPathWithNull, bootstrapperPathBytes.Length);
        bootstrapperPathWithNull[bootstrapperPathBytes.Length] = 0;

        bool success;
        fixed(byte* bootstrapperPathPtr = bootstrapperPathWithNull)
        {
            success = NativeMethods.syringe_inject(_syringe, bootstrapperPathPtr);
        }
        
        if (!success)
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