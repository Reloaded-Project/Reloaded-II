namespace Reloaded.Mod.Loader
{
    public static class Errors
    {
        public const string UnableToFindApplication = "Unable to find a configuration for the currently executing application.";
        public const string ModLoaderNotInitialized = "Mod loader has not been initialized.";
        public const string ModToLoadNotFound = "Mod to load has not been found";
        public const string ModToUnloadNotFound = "Mod to be unloaded not found.";
        public const string ModToSuspendNotFound = "Mod to be suspended not found.";
        public const string ModToResumeNotFound = "Mod to be resumed not found.";
    }
}
