namespace Reloaded.Mod.Loader.Networking
{
    public enum MessageType : byte
    {
        // Parameters
        ResumeModParams = 0,
        SuspendModParams,
        UnloadModParams,
        LoadModParams,
        GetLoadedModsParams,

        // Responses
        GetLoadedModsResponse = 128,
    }
}
