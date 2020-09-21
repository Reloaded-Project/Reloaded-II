using System;
using System.Diagnostics;
using Reloaded.Mod.Launcher.Pages.Dialogs;
using Reloaded.Mod.Launcher.Utility;

namespace Reloaded.Mod.Launcher.Misc
{
    public static class Errors
    {
        public static string Error()                                => ApplicationResourceAcquirer.GetTypeOrAlternative("ErrorError", "Error");
        public static string UnknownError()                         => ApplicationResourceAcquirer.GetTypeOrAlternative("ErrorUnknown", "Unknown Error");
        public static string PathNullOrEmpty()                      => ApplicationResourceAcquirer.GetTypeOrAlternative("ErrorPathNullOrEmpty", "The path to this application is either null or empty. Please fix the path.");
        public static string FailedToGetDirectoryOfMod()            => ApplicationResourceAcquirer.GetTypeOrAlternative("ErrorFailedToGetDirectoryOfMod", "Failed to get directory of mod to delete.");
        public static string FailedToGetDirectoryOfApplication()    => ApplicationResourceAcquirer.GetTypeOrAlternative("ErrorFailedToGetDirectoryOfApplication", "Failed to get directory of application to delete.");
        public static string LoaderNotFound()                       => ApplicationResourceAcquirer.GetTypeOrAlternative("ErrorLoaderNotFound", "was not found. This may be caused by Antivirus software deleting and/or breaking changes in GitHub repository.");
        public static string FailedToStartProcess()                 => ApplicationResourceAcquirer.GetTypeOrAlternative("ErrorFailedToStartProcess", "Failed to start the process. Is your path correct?");
        public static string DllInjectionFailed()                   => ApplicationResourceAcquirer.GetTypeOrAlternative("ErrorDllInjectionFailed", "Failed to DLL inject into application process.");
        public static string PathToApplicationInvalid()             => ApplicationResourceAcquirer.GetTypeOrAlternative("ErrorPathToApplicationInvalid", "The path to the application to be launched is invalid. Please re-check your application configuration.");
        public static string ErrorGetProcAddress32Failed()             => ApplicationResourceAcquirer.GetTypeOrAlternative("ErrorGetProcAddress32Failed", "Warning: You are probably missing the x86 version of .NET Core, 32-bit applications will not work.\nPlease re-check the requirements listed on the download page.\nActual Error: Failed to acquire address of GetProcAddress for x86.");

        /// <summary>
        /// Handles a generic thrown exception.
        /// </summary>
        public static void HandleException(Exception ex, string message = "")
        {
            if (!Debugger.IsAttached)
            {
                ActionWrappers.ExecuteWithApplicationDispatcher(() =>
                {
                    var messageBox = new MessageBox(Errors.UnknownError(), $"{message}{ex.Message}\n{ex.StackTrace}");
                    messageBox.ShowDialog();
                });
            }
            else
            {
                Debugger.Break();
            }
        }
    }
}
