using System;
using System.Collections.Generic;
using System.Text;

namespace Reloaded.Mod.Interfaces.Internal
{
    public interface IModConfigV3 : IModConfigV2
    {
        /// <summary>
        /// Gets the path to the native DLL Path for 32-bit systems.
        /// </summary>
        string ModNativeDll32 { get; set; }

        /// <summary>
        /// Gets the path to the native DLL Path for 64-bit systems.
        /// </summary>
        string ModNativeDll64 { get; set; }

        /// <summary>
        /// Gets the path of the Managed DLL for x86 if the project is built as a Ready To Run image.
        /// </summary>
        string ModR2RManagedDll32 { get; set; }

        /// <summary>
        /// Gets the path of the Managed DLL for x64 if the project is built as a Ready To Run image.
        /// </summary>
        string ModR2RManagedDll64 { get; set; }

        /// <summary>
        /// Returns true if the mod consists of ReadyToRun (R2R) executables, else false.
        /// </summary>
        /// <param name="configPath">The full path to the <see cref="ConfigFileName"/> configuration file. (See <see cref="PathGenericTuple{TGeneric}"/>)</param>
        bool IsR2R(string configPath);

        /// <summary>
        /// Returns true if the mod is native, else false.
        /// </summary>
        /// <param name="configPath">The full path to the <see cref="ConfigFileName"/> configuration file. (See <see cref="PathGenericTuple{TGeneric}"/>)</param>
        bool IsNativeMod(string configPath);

        /// <summary>
        /// Retrieves the path to the individual DLL for this mod.
        /// </summary>
        /// <param name="configPath">The full path to the <see cref="ConfigFileName"/> configuration file. (See <see cref="PathGenericTuple{TGeneric}"/>)</param>
        string GetManagedDllPath(string configPath);

        /// <summary>
        /// Retrieves the path to the native 32-bit DLL for this mod, autodetecting if 32 or 64 bit..
        /// </summary>
        /// <param name="configPath">The full path to the <see cref="ConfigFileName"/> configuration file. (See <see cref="PathGenericTuple{TGeneric}"/>)</param>
        string GetNativeDllPath(string configPath);

        /// <summary>
        /// Tries to retrieve the full path to the icon that represents this mod.
        /// </summary>
        /// <param name="configPath">The full path to the <see cref="ConfigFileName"/> configuration file. (See <see cref="PathGenericTuple{TGeneric}"/>)</param>
        /// <param name="iconPath">Full path to the icon.</param>
        bool TryGetIconPath(string configPath, out string iconPath);
    }
}
