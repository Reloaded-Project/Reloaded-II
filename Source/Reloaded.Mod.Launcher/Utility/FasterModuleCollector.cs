using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using Reloaded.Injector.Exceptions;

namespace Reloaded.Mod.Launcher.Utility
{
    internal static class FasterModuleCollector
    {
        private static StringBuilder _modulePathBuilder = new StringBuilder(32767);

        /// <exception cref="DllInjectorException">Bytes to fill module list returned 0. The process is probably not yet initialized.</exception>
        public static List<string> CollectModuleNames(Process process)
        {
            List<string> collectedModuleNames = new List<string>(1000);
            IntPtr[] modulePointers = new IntPtr[0];
            int numberOfModules;
            int bytesNeeded;

            // Determine number of modules.
            if (!EnumProcessModulesEx(process.Handle, modulePointers, 0, out bytesNeeded, (uint)ModuleFilter.ListModulesAll))
                return collectedModuleNames;

            if (bytesNeeded == 0)
                throw new DllInjectorException("Bytes needed to dump module list returned 0. This means that either the process probably not yet fully initialized.");

            numberOfModules = bytesNeeded / IntPtr.Size;
            modulePointers  = new IntPtr[numberOfModules];

            // Collect modules from the process
            if (EnumProcessModulesEx(process.Handle, modulePointers, bytesNeeded, out bytesNeeded, (uint)ModuleFilter.ListModulesAll))
            {
                for (int x = 0; x < numberOfModules; x++)
                {
                    GetModuleFileNameEx(process.Handle, modulePointers[x], _modulePathBuilder, (uint)(_modulePathBuilder.Capacity));
                    collectedModuleNames.Add(_modulePathBuilder.ToString());
                }
            }

            return collectedModuleNames;
        }

        internal enum ModuleFilter
        {
            ListModulesDefault = 0x0,
            ListModules32Bit = 0x01,
            ListModules64Bit = 0x02,
            ListModulesAll = 0x03,
        }

        [SuppressUnmanagedCodeSecurity]
        [DllImport("psapi.dll")]
        private static extern bool EnumProcessModulesEx(IntPtr hProcess, IntPtr[] lphModule, int cb, out int lpcbNeeded, uint dwFilterFlag);

        [SuppressUnmanagedCodeSecurity]
        [DllImport("psapi.dll")]
        private static extern uint GetModuleFileNameEx(IntPtr hProcess, IntPtr hModule, [Out] StringBuilder lpBaseName, uint nSize);
    }
}
