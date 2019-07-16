namespace Reloaded.Mod.Loader.Server.Messages
{
    public enum MessageType : byte
    {
        // Parameters | 0 - 128
        ResumeMod = 0,
        SuspendMod,
        UnloadMod,
        LoadMod,
        GetLoadedMods,

        // Responses | 128 - 200
        Acknowledgement         = 128, // Acknowledgement of any action without a real return value.
        GetLoadedModsResponse   = 129,

        // Exceptions (Responses) | 200 - 255 
        GenericException = 200
    }
}
