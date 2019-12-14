using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Input;
using Ookii.Dialogs.Wpf;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Launcher.Models.ViewModel;
using Reloaded.Mod.Loader.IO.Config;
using Reloaded.WPF.Utilities;

namespace Reloaded.Mod.Launcher.Commands.AddAppPage
{
    /// <summary>
    /// Command to be used by the <see cref="AddAppPage"/> which allows
    /// for the addition of a new application.
    /// </summary>
    public class AddApplicationCommand : ICommand
    {
        private XamlResource<string> _xamlAddAppExecutableTitle = new XamlResource<string>("AddAppExecutableTitle");
        private XamlResource<string> _xamlAddAppExecutableFilter = new XamlResource<string>("AddAppExecutableFilter");
        private readonly AddAppViewModel _addAppViewModel;

        public AddApplicationCommand()
        {
            _addAppViewModel = IoC.Get<AddAppViewModel>();
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            // Select EXE
            string exePath = SelectEXEFile();

            if (String.IsNullOrEmpty(exePath) || ! File.Exists(exePath))
                return;

            // Get file information and populate initial application details.
            var fileInfo = FileVersionInfo.GetVersionInfo(exePath);
            IApplicationConfig config = new ApplicationConfig(Path.GetFileName(fileInfo.FileName).ToLower(), fileInfo.ProductName, exePath);

            // Set AppName if empty & Ensure no duplicate ID.
            if (String.IsNullOrEmpty(config.AppName))
                config.AppName = config.AppId;

            UpdateIdIfDuplicate(config);

            // Get paths.
            var loaderConfig = IoC.Get<LoaderConfig>();
            string applicationConfigDirectory = loaderConfig.ApplicationConfigDirectory;
            string applicationDirectory = Path.Combine(applicationConfigDirectory, config.AppId);
            string applicationConfigFile = Path.Combine(applicationDirectory, ApplicationConfig.ConfigFileName);

            // Write file to disk.
            Directory.CreateDirectory(applicationDirectory);
            ApplicationConfig.WriteConfiguration(applicationConfigFile, (ApplicationConfig)config);
        }

        public event EventHandler CanExecuteChanged = (sender, args) => { };

        /// <summary>
        /// Checks the ID if it already exists and modifies the ID if it does so.
        /// </summary>
        private void UpdateIdIfDuplicate(IApplicationConfig config)
        {
            // Ensure no duplication of AppId
            while (_addAppViewModel.MainPageViewModel.Applications.Any(x => x.ApplicationConfig.AppId == config.AppId))
            {
                config.AppId += "_dup";
            }
        }

        /// <summary>
        /// Opens up a file selection dialog allowing for the selection of an executable to associate with the profile.
        /// </summary>
        private string SelectEXEFile()
        {
            var dialog = new VistaOpenFileDialog();
            dialog.Title = _xamlAddAppExecutableTitle.Get();
            dialog.Filter = $"{_xamlAddAppExecutableFilter.Get()} (*.exe)|*.exe";

            if ((bool) dialog.ShowDialog())
            {
                return dialog.FileName;
            }

            return "";
        }
    }
}
