using System;
using Reloaded.Mod.Loader.Bootstrap;
using Reloaded.Mod.Loader.Networking;
using Reloaded.Mod.Loader.Networking.Messages;
using Reloaded.Mod.Loader.Server;
using Reloaded.Mod.Loader.Server.Structs;

namespace Reloaded.Mod.Loader
{
    public static class API
    {
        private static Server.SimpleHost<MessageType> _simpleHost;

        /* Ensures DLL Resolution */
        static API()
        {
            AppDomain.CurrentDomain.AssemblyResolve += LocalAssemblyResolver.ResolveAssembly;
        }

        /* Initialize Mod Loader (DLL_PROCESS_ATTACH) */

        /// <summary>
        /// Returns the port on the local machine
        /// </summary>
        public static int GetPort()
        {
            return _simpleHost.Port;
        }

        /* Message Handlers */
        private static void GetLoadedMods(NetMessage<GetLoadedModsParams> obj)
        {
            throw new NotImplementedException();
        }


        /*
            ModInfo = GetLoadedMods()
            SuspendMod(string ModId)
            ResumeMod(string ModId)
            UnloadMod(string ModId)
            LoadMod(string ModId)

            // Each has a message and result structs.
            // All structs in enum (MessageType) and inherit from base message.

           
        */
    }
}
