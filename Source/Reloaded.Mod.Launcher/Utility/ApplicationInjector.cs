using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using Reloaded.Mod.Loader.IO;
using Reloaded.Mod.Loader.IO.Config;
using Reloaded.Mod.Loader.Server;
using Reloaded.WPF.Utilities;

namespace Reloaded.Mod.Launcher.Utility
{
    /// <summary>
    /// Injects Reloaded into an active process.
    /// </summary>
    public class ApplicationInjector
    {
        private static XamlResource<int> _xamlModLoaderSetupTimeout     = new XamlResource<int>("AppLauncherModLoaderSetupTimeout");
        private static XamlResource<int> _xamlModLoaderSetupSleepTime   = new XamlResource<int>("AppLauncherModLoaderSetupSleepTime");

        static ApplicationInjector()
        {
            _xamlModLoaderSetupTimeout.DefaultValue = 30000;
            _xamlModLoaderSetupSleepTime.DefaultValue = 32;
        }

        private Process _process;
        private BasicDllInjector _injector;

        public ApplicationInjector(Process process)
        {
            _process = process;
            _injector = new BasicDllInjector(process);
        }

        /// <summary>
        /// Injects the Reloaded bootstrapper into an active process.
        /// </summary>
        /// <exception cref="ArgumentException">DLL Injection failed, likely due to bad DLL or application.</exception>
        public void Inject()
        {
            long handle = _injector.Inject(GetBootstrapperPath(_process));
            if (handle == 0)
                throw new ArgumentException(Errors.DllInjectionFailed());

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
                // Exit if application crashes while loading Reloaded..
                if (_process.HasExited)
                    return 0;

                return Client.GetPort((int)_process.Id);
            }, WhileCondition, _xamlModLoaderSetupTimeout.Get(), _xamlModLoaderSetupSleepTime.Get());
        }

        private string GetBootstrapperPath(Process process)
        {
            // If the LoaderConfig is not bound, read it from disk.
            if (! IoC.IsExplicitlyBound<LoaderConfig>())
            {
                if (process.Is64Bit())
                    return LoaderConfigReader.ReadConfiguration().Bootstrapper64Path;
                else
                    return LoaderConfigReader.ReadConfiguration().Bootstrapper32Path;
            }
            else
            {
                if (process.Is64Bit())
                    return IoC.Get<LoaderConfig>().Bootstrapper64Path;
                else
                    return IoC.Get<LoaderConfig>().Bootstrapper32Path;
            }
        }

        /* Native Imports */
        [DllImport("Kernel32.dll", SetLastError = true, ExactSpelling = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CheckRemoteDebuggerPresent(IntPtr hProcess, [MarshalAs(UnmanagedType.Bool)] out bool isDebuggerPresent);
    }
}
