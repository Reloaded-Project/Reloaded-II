using System;
using System.IO;
using System.Runtime.InteropServices;
using Indieteur.SAMAPI;
using Reloaded.Hooks.Definitions;
using Reloaded.Mod.Interfaces;

namespace Reloaded.Mod.Loader.Utilities.Steam
{
    public class SteamHook
    {
        /* Constants */
        public const string SteamAPI32 = "steam_api.dll";
        public const string SteamAPI64 = "steam_api64.dll";

        public const string RestartAppIfNecessaryName = "SteamAPI_RestartAppIfNecessary";
        public const string IsSteamRunningName        = "SteamAPI_IsSteamRunning";

        /* Hook */
        private IHook<RestartAppIfNecessary> _restartAppIfNecessaryHook;  // Newer games
        private IHook<IsSteamRunning> _isSteamRunningHook;                // Older games
        private string _applicationFolder;

        /* Setup */
        public SteamHook(IReloadedHooks hooks, ILogger logger, string applicationFolder)
        {
            _applicationFolder = applicationFolder;
            var steamApiPath = Environment.Is64BitProcess ? Path.GetFullPath(SteamAPI64) : Path.GetFullPath(SteamAPI32);
            if (!File.Exists(steamApiPath))
                return;

            // Hook relevant functions
            var libraryAddress = Native.Kernel32.LoadLibraryW(steamApiPath);
            _restartAppIfNecessaryHook = HookExportedFunction<RestartAppIfNecessary>(libraryAddress, RestartAppIfNecessaryName, RestartAppIfNecessaryImpl);
            _isSteamRunningHook = HookExportedFunction<IsSteamRunning>(libraryAddress, IsSteamRunningName, IsSteamRunningImpl);

            // Drop steam_appid.txt
            DropSteamAppId(logger);

            // Local function.
            IHook<T> HookExportedFunction<T>(IntPtr libraryHandle, string functionName, T handler)
            {
                var functionPtr = Native.Kernel32.GetProcAddress(libraryHandle, functionName);
                if (functionPtr == IntPtr.Zero)
                {
                    logger.SteamWriteLineAsync($"{functionName} not found.", logger.ColorYellowLight);
                    return null;
                }

                logger.SteamWriteLineAsync($"{functionName} hooked successfully.", logger.ColorGreenLight);
                return hooks.CreateHook<T>(handler, (long)functionPtr).Activate();
            }
        }

        private void DropSteamAppId(ILogger logger)
        {
            try
            {
                var manager = new SteamAppsManager();
                foreach (var app in manager.SteamApps)
                {
                    if (!_applicationFolder.Contains(app.InstallDir))
                        continue;

                    logger.SteamWriteLineAsync($"Found Steam Library Entry with Id {app.AppID}. Dropping {SteamAppId.FileName}.", logger.ColorGreenLight);
                    SteamAppId.WriteToDirectory(_applicationFolder, app.AppID);
                    return;
                }

                logger.SteamWriteLineAsync($"Application not found in any Steam library. Recommend dropping a {SteamAppId.FileName} yourself.", logger.ColorRedLight);
            }
            catch (Exception e)
            {
                logger.SteamWriteLineAsync($"Failed to scan through Steam games and locate current game. Error: {e.Message}, {e.StackTrace}", logger.ColorRedLight);
            }
        }

        private bool IsSteamRunningImpl()
        {
            _isSteamRunningHook.OriginalFunction();
            return true;
        }

        private bool RestartAppIfNecessaryImpl(uint appid)
        {
            // Write the Steam AppID to a local file if not dropped by the other method.
            _restartAppIfNecessaryHook.OriginalFunction(appid);
            SteamAppId.WriteToDirectory(_applicationFolder, (int)appid);

            return false;
        }

        /*
            See: https://partner.steamgames.com/doc/api/steam_api#SteamAPI_RestartAppIfNecessary
        */

        [Hooks.Definitions.X64.Function(Hooks.Definitions.X64.CallingConventions.Microsoft)]
        [Hooks.Definitions.X86.Function(Hooks.Definitions.X86.CallingConventions.Cdecl)]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate bool RestartAppIfNecessary(uint appId);

        [Hooks.Definitions.X64.Function(Hooks.Definitions.X64.CallingConventions.Microsoft)]
        [Hooks.Definitions.X86.Function(Hooks.Definitions.X86.CallingConventions.Cdecl)]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate bool IsSteamRunning();
    }
}
