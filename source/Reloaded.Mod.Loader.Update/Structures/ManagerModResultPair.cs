using Reloaded.Mod.Loader.IO.Config;
using Reloaded.Mod.Loader.IO.Structs;
using Sewer56.Update;
using Sewer56.Update.Packaging.Structures;
using Sewer56.Update.Structures;

namespace Reloaded.Mod.Loader.Update.Structures
{
    public class ManagerModResultPair
    {
        public UpdateManager<Empty> Manager { get; private set; }

        public CheckForUpdatesResult Result { get; private set; }

        public PathTuple<ModConfig> ModTuple { get; private set; }

        public ManagerModResultPair(UpdateManager<Empty> manager, CheckForUpdatesResult result, PathTuple<ModConfig> modTuple)
        {
            Manager = manager;
            Result = result;
            ModTuple = modTuple;
        }
    }
}
