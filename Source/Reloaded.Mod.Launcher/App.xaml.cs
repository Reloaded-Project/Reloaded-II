using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Windows;
using Reloaded.Mod.Launcher.Misc;
using Reloaded.Mod.Launcher.Utility;
using Reloaded.Mod.Loader.IO.Config;

namespace Reloaded.Mod.Launcher
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private Dictionary<string, string> _commandLineArguments = new Dictionary<string, string>();

        /// <summary>
        /// Entry point for the application.
        /// </summary>
        public App()
        {
            PopulateCommandLineArgs();
            if (_commandLineArguments.TryGetValue(Constants.ParameterKill, out string processId))
            {
                var process = Process.GetProcessById(Convert.ToInt32(processId));
                process.Kill();

                ActionWrappers.SleepOnConditionWithTimeout(() => process.HasExited, 1000, 32);
            }

            if (_commandLineArguments.TryGetValue(Constants.ParameterLaunch, out string applicationToLaunch))
            {
                // Acquire arguments
                _commandLineArguments.TryGetValue(Constants.ParameterArguments, out var arguments);
                if (arguments == null)
                    arguments = "";

                applicationToLaunch = Path.GetFullPath(applicationToLaunch);
                var application = ApplicationConfig.GetAllApplications().FirstOrDefault(x => Path.GetFullPath(x.Object.AppLocation) == applicationToLaunch);
                if (application != null)
                    arguments = $"{arguments} {application.Object.AppArguments}";

                // Launch the application.
                var launcher = ApplicationLauncher.FromLocationAndArguments(applicationToLaunch, arguments);
                launcher.Start();

                // Quit the process.
                Environment.Exit(0);
            }

            if (!IsElevated)
            {
                MessageBox.Show("You need to run this application as administrator.\n" +
                                "Administrative privileges are needed to receive application launch/exit events " +
                                "from Windows Management Instrumentation (WMI).\n" +
                                "Developers: Run your favourite IDE e.g. Visual Studio as Admin.");
                Environment.Exit(0);
            }
        }

        private void PopulateCommandLineArgs()
        {
            string[] args = Environment.GetCommandLineArgs();
            for (int index = 1; index < args.Length; index += 2)
                _commandLineArguments.Add(args[index], args[index + 1]);
        }

        /// <summary>
        /// Checks if the application is running as root/sudo/administrator.
        /// </summary>
        private static bool IsElevated =>
            new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);
    }
}
