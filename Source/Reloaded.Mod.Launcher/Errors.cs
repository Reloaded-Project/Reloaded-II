using Reloaded.Mod.Launcher.Utility;

namespace Reloaded.Mod.Launcher
{
    public static class Errors
    {
        public static string Error()                                => ApplicationResourceAcquirer.GetTypeOrAlternative("ErrorError", "Error");
        public static string PathNullOrEmpty()                      => ApplicationResourceAcquirer.GetTypeOrAlternative("PathNullOrEmpty", "The path to this application is either null or empty. Please fix the path.");
        public static string FailedToGetDirectoryOfMod()            => ApplicationResourceAcquirer.GetTypeOrAlternative("FailedToGetDirectoryOfMod", "Failed to get directory of mod to delete.");
        public static string FailedToGetDirectoryOfApplication()    => ApplicationResourceAcquirer.GetTypeOrAlternative("FailedToGetDirectoryOfApplication", "Failed to get directory of application to delete.");
        public static string LoaderNotFound()                       => ApplicationResourceAcquirer.GetTypeOrAlternative("LoaderNotFound", "was not found. This may be caused by Antivirus software deleting and/or breaking changes in Github repository.");
        public static string FailedToStartProcess()                 => ApplicationResourceAcquirer.GetTypeOrAlternative("FailedToStartProcess", "Failed to start the process. Is your path correct?");


    }
}
