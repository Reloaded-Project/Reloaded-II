namespace Reloaded.Mod.Loader
{
    public static class Errors
    {
        public const string UnableToFindApplication = "Unable to find a configuration for the currently executing application.";
        public const string ModLoaderNotInitialized = "Mod loader has not been initialized.";
        public const string ModLoaderAlreadyInitialized = "Mod loader is already initialized.";

        public static string ModToLoadNotFound(string modId)        => $"[Load] Mod with ID ({modId}) not found.";
        public static string ModToUnloadNotFound(string modId)      => $"[Unload] Mod with ID ({modId}) not found.";
        public static string ModToSuspendNotFound(string modId)     => $"[Suspend] Mod with ID ({modId}) not found.";
        public static string ModToResumeNotFound(string modId)      => $"[Resume] Mod with ID ({modId}) not found.";

        public static string ModSuspendNotSupported(string modId)   => $"Suspend/Resume is not supported by this mod. ({modId})";
        public static string ModUnloadNotSupported(string modId)    => $"Load/Unload is not supported by this mod. ({modId})";

        public static string ModAlreadyLoaded(string modId)         => $"Mod with specified ID ({modId}) is already loaded.";
    }
}
