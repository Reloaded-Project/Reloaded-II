using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Security.Principal;
using System.Windows;
using Reloaded.Mod.Launcher.Misc;
using Reloaded.Mod.Launcher.Pages.Dialogs;
using Reloaded.Mod.Launcher.Utility;
using Reloaded.Mod.Loader.IO;
using Reloaded.Mod.Loader.IO.Config;
using static System.Environment;
using Constants = Reloaded.Mod.Launcher.Misc.Constants;

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

        /// <summary>
        /// Allows for selection of an individual language.
        /// </summary>
        public static XamlFileSelector LanguageSelector { get; private set; }

        /// <summary>
        /// Allows for selection of an individual theme.
        /// </summary>
        public static XamlFileSelector ThemeSelector { get; private set; }

        private Dictionary<string, string> _commandLineArguments = new Dictionary<string, string>();

        /// <summary>
        /// Entry point for the application.
        /// </summary>
        public App()
        {
            PopulateCommandLineArgs();

            // Check if Kill Process
            if (_commandLineArguments.TryGetValue(Constants.ParameterKill, out string processId))
                KillProcessWithId(processId);

            // Check if Launch
            if (_commandLineArguments.TryGetValue(Constants.ParameterLaunch, out string applicationToLaunch))
                LaunchApplicationAndExit(applicationToLaunch);

            // Move Contents involving UI elements to LoadCompleted
            this.Startup += OnStartup;
        }

        private void OnStartup(object sender, StartupEventArgs e)
        {
            EnableProfileOptimization();
            SetupResources();

            // Check if Download Mod
            if (_commandLineArguments.TryGetValue(Constants.ParameterDownload, out string downloadUrl))
                DownloadModAndExit(downloadUrl);

            _commandLineArguments = null;
            this.Startup -= OnStartup;
        }

        private void SetupResources()
        {
            // Ideally this should be in Setup, however the download dialogs should be localized.
            var launcherFolder = Path.GetDirectoryName(GetCommandLineArgs()[0]);
            LanguageSelector = new XamlFileSelector($"{launcherFolder}\\Assets\\Languages");
            ThemeSelector = new XamlFileSelector($"{launcherFolder}\\Theme");
            Resources.MergedDictionaries.Add(LanguageSelector);
            Resources.MergedDictionaries.Add(ThemeSelector);
        }

        // Excluding most likely unused code from JIT
        [MethodImpl(MethodImplOptions.NoInlining)]
        private void LaunchApplicationAndExit(string applicationToLaunch)
        {
            // Acquire arguments
            _commandLineArguments.TryGetValue(Constants.ParameterArguments, out var arguments);
            if (arguments == null)
                arguments = "";

            applicationToLaunch = Path.GetFullPath(applicationToLaunch);
            var application = ApplicationConfig.GetAllApplications(IoC.Get<LoaderConfig>().ApplicationConfigDirectory).FirstOrDefault(x => Path.GetFullPath(x.Config.AppLocation) == applicationToLaunch);
            if (application != null)
                arguments = $"{arguments} {application.Config.AppArguments}";

            // Show warning for Wine users.
            if (Shared.Environment.IsWine)
            {
                // Set up UI Resources, since they're needed for the dialog.
                SetupResources();
                if (CompatibilityDialogs.WineShowLaunchDialog())
                    StartGame(applicationToLaunch, arguments);
            }
            else
            {
                StartGame(applicationToLaunch, arguments);
            }
        }

        private static void StartGame(string applicationToLaunch, string arguments)
        {
            // Launch the application.
            var launcher = ApplicationLauncher.FromLocationAndArguments(applicationToLaunch, arguments);
            launcher.Start();

            // Quit the process.
            Environment.Exit(0);
        }

        // Excluding most likely unused code from JIT
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void DownloadModAndExit(string downloadUrl)
        {
            if (downloadUrl.StartsWith($"{Constants.ReloadedProtocol}:", StringComparison.InvariantCultureIgnoreCase))
                downloadUrl = downloadUrl.Substring(Constants.ReloadedProtocol.Length + 1);

            var dialog = new DownloadModArchiveDialog(new[] {new Uri(downloadUrl)});
            dialog.ShowDialog();

            // Quit the process.
            Environment.Exit(0);
        }

        private void KillProcessWithId(string processId)
        {
            var process = Process.GetProcessById(Convert.ToInt32(processId));
            process.Kill();

            ActionWrappers.SleepOnConditionWithTimeout(() => process.HasExited, 1000, 32);
        }

        private void EnableProfileOptimization()
        {
            // Start Profile Optimization
            var profileRoot = Paths.ProfileOptimizationPath;
            Directory.CreateDirectory(profileRoot);

            // Define the folder where to save the profile files
            ProfileOptimization.SetProfileRoot(profileRoot);

            // Start profiling and save it in Startup.profile
            ProfileOptimization.StartProfile("Launcher.profile");
        }

        private void PopulateCommandLineArgs()
        {
            string[] args = GetCommandLineArgs();
            for (int index = 1; index < args.Length; index += 2)
                _commandLineArguments.Add(args[index], args[index + 1]);
        }
    }
}
