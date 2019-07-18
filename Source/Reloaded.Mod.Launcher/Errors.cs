using Reloaded.Mod.Launcher.Utility;

namespace Reloaded.Mod.Launcher
{
    public static class Errors
    {
        public static string Error()                                => ApplicationResourceAcquirer.GetTypeOrAlternative("ErrorError", "Error");
        public static string PathNullOrEmpty()                      => ApplicationResourceAcquirer.GetTypeOrAlternative("ErrorPathNullOrEmpty", "The path to this application is either null or empty. Please fix the path.");
        public static string FailedToGetDirectoryOfMod()            => ApplicationResourceAcquirer.GetTypeOrAlternative("ErrorFailedToGetDirectoryOfMod", "Failed to get directory of mod to delete.");
        public static string FailedToGetDirectoryOfApplication()    => ApplicationResourceAcquirer.GetTypeOrAlternative("ErrorFailedToGetDirectoryOfApplication", "Failed to get directory of application to delete.");
        public static string LoaderNotFound()                       => ApplicationResourceAcquirer.GetTypeOrAlternative("ErrorLoaderNotFound", "was not found. This may be caused by Antivirus software deleting and/or breaking changes in Github repository.");
        public static string FailedToStartProcess()                 => ApplicationResourceAcquirer.GetTypeOrAlternative("ErrorFailedToStartProcess", "Failed to start the process. Is your path correct?");
        public static string DllInjectionFailed()                   => ApplicationResourceAcquirer.GetTypeOrAlternative("ErrorDllInjectionFailed", "Failed to DLL inject into application process.");
        public static string PathToApplicationInvalid()             => ApplicationResourceAcquirer.GetTypeOrAlternative("ErrorPathToApplicationInvalid", "The path to the application to be launched is invalid. Please re-check your application configuration.");
    }
}
