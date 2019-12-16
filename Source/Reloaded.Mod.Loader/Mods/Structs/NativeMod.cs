using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Interfaces.Internal;

namespace Reloaded.Mod.Loader.Mods.Structs
{
    /// <summary>
    /// A wrapper for Reloaded's <see cref="IMod"/> interface for a native DLL.
    /// </summary>
    public class NativeMod : IMod
    {
        /// <summary>
        /// Handle to the native module.
        /// </summary>
        private IntPtr _moduleHandle;

        private ReloadedStart _start;
        private ReloadedSuspend _reloadedSuspend;
        private ReloadedResume _reloadedResume;
        private ReloadedUnload _reloadedUnload;
        private ReloadedCanSuspend _reloadedCanSuspend;
        private ReloadedCanUnload _reloadedCanUnload;
        private InitializeASI _initializeAsi;
        private Init _init;
        private bool _started;

        /// <summary>
        /// Creates an IMod wrapper for a native DLL.
        /// </summary>
        /// <param name="path">Path to the native DLL.</param>
        public NativeMod(string path)
        {
            _moduleHandle = LoadLibraryW(path);
            _start = GetDelegateForNativeFunction<ReloadedStart>(_moduleHandle, nameof(ReloadedStart));
            _reloadedSuspend = GetDelegateForNativeFunction<ReloadedSuspend>(_moduleHandle, nameof(ReloadedSuspend));
            _reloadedResume = GetDelegateForNativeFunction<ReloadedResume>(_moduleHandle, nameof(ReloadedResume));
            _reloadedUnload = GetDelegateForNativeFunction<ReloadedUnload>(_moduleHandle, nameof(ReloadedUnload));
            _reloadedCanSuspend = GetDelegateForNativeFunction<ReloadedCanSuspend>(_moduleHandle, nameof(ReloadedCanSuspend));
            _reloadedCanUnload = GetDelegateForNativeFunction<ReloadedCanUnload>(_moduleHandle, nameof(ReloadedCanUnload));
            _initializeAsi = GetDelegateForNativeFunction<InitializeASI>(_moduleHandle, nameof(InitializeASI));
            _init = GetDelegateForNativeFunction<Init>(_moduleHandle, nameof(Init));
        }

        // Note for implementation: There is no guarantee mod exports any function (Start, CanUnload, etc.). Start function might just be DllMain.

        public void Start(IModLoaderV1 loader)
        {
            // Try Reloaded Entry point and then others.
            if (_start != null)
            {
                _start.Invoke();
                _started = true;
            }
            else if (_initializeAsi != null && !_started)
            {
                _initializeAsi.Invoke();
                _started = true;
            }
            else if (_init != null && !_started)
            {
                _init.Invoke();
                _started = true;
            }
        }

        public void Suspend() => _reloadedSuspend?.Invoke();
        public void Resume() => _reloadedResume?.Invoke();
        public void Unload() => _reloadedUnload?.Invoke();
        public bool CanUnload() => _reloadedCanUnload?.Invoke() ?? false;
        public bool CanSuspend() => _reloadedCanSuspend?.Invoke() ?? false;

        public Action Disposing { get; }

        // Utility Functions.
        private TDelegate GetDelegateForNativeFunction<TDelegate>(IntPtr moduleHandle, string functionName) where TDelegate : Delegate
        {
            var address = GetProcAddress(moduleHandle, functionName);
            return address != IntPtr.Zero ? Marshal.GetDelegateForFunctionPointer<TDelegate>(address) : null;
        }

        // Delegates for native Other Exports.
        private delegate void InitializeASI();
        private delegate void Init();

        // Delegates for native Reloaded Exports.
        private delegate void ReloadedStart();
        private delegate void ReloadedSuspend();
        private delegate void ReloadedResume();
        private delegate void ReloadedUnload();
        private delegate bool ReloadedCanUnload();
        private delegate bool ReloadedCanSuspend();

        #region Native Imports
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern IntPtr LoadLibraryW(string lpFileName);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Ansi)]
        public static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);
        #endregion
    }
}
