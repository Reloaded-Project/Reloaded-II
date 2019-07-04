namespace Reloaded.Mod.Loader.Mods.Structs
{
    /// <summary>
    /// A tuple that stores the path of a mod DLL file and its modId.
    /// </summary>
    public struct ModPathIdTuple
    {
        public string ModId { get; private set; }
        public string ModPath { get; private set; }

        public ModPathIdTuple(string modId, string modPath)
        {
            ModId = modId;
            ModPath = modPath;
        }
    }
}
