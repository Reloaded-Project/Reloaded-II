namespace Reloaded.Mod.Loader.Server.Messages
{
    public enum MessageType : byte
    {
        // Parameters
        ResumeMod = 0,
        SuspendMod,
        UnloadMod,
        LoadMod,
        GetLoadedMods,

        // Responses
        GetLoadedModsResponse = 128,

        // Exceptions (Responses)
        GenericException = 255
    }
}
