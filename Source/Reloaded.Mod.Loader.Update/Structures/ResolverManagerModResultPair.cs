using Onova;
using Onova.Models;
using Reloaded.Mod.Loader.IO.Config;
using Reloaded.Mod.Loader.IO.Structs;
using Reloaded.Mod.Loader.Update.Interfaces;

namespace Reloaded.Mod.Loader.Update.Structures
{
    public class ResolverManagerModResultPair
    {
        public IModResolver Resolver { get; private set; }
        public IUpdateManager Manager { get; private set; }
        public CheckForUpdatesResult Result { get; private set; }
        public PathGenericTuple<ModConfig> ModTuple { get; private set; }

        public ResolverManagerModResultPair(IModResolver resolver, IUpdateManager manager, CheckForUpdatesResult result, PathGenericTuple<ModConfig> modTuple)
        {
            Resolver = resolver;
            Manager = manager;
            Result = result;
            ModTuple = modTuple;
        }
    }
}
