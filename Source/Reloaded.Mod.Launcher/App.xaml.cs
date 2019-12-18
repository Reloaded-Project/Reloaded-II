using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Threading;
using System.Windows;
using System.Windows.Navigation;
using Reloaded.Mod.Launcher.Misc;
using Reloaded.Mod.Launcher.Pages.Dialogs;
using Reloaded.Mod.Launcher.Utility;
using Reloaded.Mod.Loader.IO.Config;
using MessageBox = System.Windows.MessageBox;

namespace Reloaded.Mod.Launcher
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Returns if the application is running as root/sudo/administrator.
        /// </summary>
        public static bool IsElevated { get; } = new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);
        private Dictionary<string, string> _commandLineArguments = new Dictionary<string, string>();

        /// <summary>
        /// Entry point for the application.
        /// </summary>
        public App()
        {
            PopulateCommandLineArgs();

            /* Check if Kill Process */
            if (_commandLineArguments.TryGetValue(Constants.ParameterKill, out string processId))
            {
                var process = Process.GetProcessById(Convert.ToInt32(processId));
                process.Kill();

                ActionWrappers.SleepOnConditionWithTimeout(() => process.HasExited, 1000, 32);
            }

            /* Check if Launch Process */
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

            // Move Contents involving UI elements to LoadCompleted
            this.Startup += OnStartup;
        }

        private void OnStartup(object sender, StartupEventArgs e)
        {            
            // Ideally this should be in Setup, however the download dialogs should be localized.
            SetupLocalization();

            /* Check if Download Mod */
            if (_commandLineArguments.TryGetValue(Constants.ParameterDownload, out string downloadUrl))
            {
                if (downloadUrl.StartsWith($"{Constants.ReloadedProtocol}:", StringComparison.InvariantCultureIgnoreCase))
                    downloadUrl = downloadUrl.Substring(Constants.ReloadedProtocol.Length + 1);

                var dialog = new DownloadModArchiveDialog(new[] { new Uri(downloadUrl) });
                dialog.ShowDialog();

                // Quit the process.
                Environment.Exit(0);
            }

            _commandLineArguments = null;
            this.Startup -= OnStartup;
        }

        private void PopulateCommandLineArgs()
        {
            string[] args = Environment.GetCommandLineArgs();
            for (int index = 1; index < args.Length; index += 2)
                _commandLineArguments.Add(args[index], args[index + 1]);
        }

        /// <summary>
        /// Sets up localization for the system language.
        /// </summary>
        private static void SetupLocalization()
        {
            // Set language dictionary.
            var langDict = new ResourceDictionary();
            string culture = Thread.CurrentThread.CurrentCulture.ToString();
            string languageFilePath = AppDomain.CurrentDomain.BaseDirectory + $"/Languages/{culture}.xaml";
            if (File.Exists(languageFilePath))
                langDict.Source = new Uri(languageFilePath, UriKind.Absolute);

            Current.Resources.MergedDictionaries.Add(langDict);
        }
    }
}
