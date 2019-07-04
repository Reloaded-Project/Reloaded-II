namespace Reloaded.Mod.Loader.Server.Messages.Structures
{
    public class ModInfo
    {
        public ModState State { get; set; }
        public string ModId { get; set; }

        public bool CanSuspend { get; set; }
        public bool CanUnload { get; set; }

        public ModInfo(ModState state, string modId, bool canSuspend, bool canUnload)
        {
            State = state;
            ModId = modId;
            CanSuspend = canSuspend;
            CanUnload = canUnload;
        }
    }
}
