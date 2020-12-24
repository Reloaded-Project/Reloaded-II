using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Input;
using Ookii.Dialogs.Wpf;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Launcher.Models.ViewModel;
using Reloaded.Mod.Loader.IO.Config;
using Reloaded.WPF.Utilities;

namespace Reloaded.Mod.Launcher.Commands.EditAppPage
{
    /// <summary>
    /// Command to be used by the <see cref="EditAppPage"/> which allows
    /// for the addition of a new application.
    /// </summary>
    public class AddApplicationCommand : ICommand
    {
        private XamlResource<string> _xamlAddAppExecutableTitle = new XamlResource<string>("AddAppExecutableTitle");
        private XamlResource<string> _xamlAddAppExecutableFilter = new XamlResource<string>("AddAppExecutableFilter");
        private readonly MainPageViewModel _mainPageViewModel;
        private ApplicationConfig _lastConfig;

        public AddApplicationCommand(MainPageViewModel viewModel)
        {
            _mainPageViewModel = viewModel;
        }

        public bool CanExecute(object parameter) => true;

        public void Execute(object parameter)
        {
            // Select EXE
            string exePath = SelectEXEFile();

            if (string.IsNullOrEmpty(exePath) || !File.Exists(exePath))
                return;

            // Get file information and populate initial application details.
            var fileInfo = FileVersionInfo.GetVersionInfo(exePath);
            IApplicationConfig config = new ApplicationConfig(Path.GetFileName(fileInfo.FileName).ToLower(), fileInfo.ProductName, exePath);

            // Set AppName if empty & Ensure no duplicate ID.
            if (string.IsNullOrEmpty(config.AppName))
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

            // TODO: Enter Application Screen
            _lastConfig = (ApplicationConfig)config;
            _mainPageViewModel.ApplicationsChanged += ApplicationsChanged;
        }

        private void ApplicationsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var newConfig = _mainPageViewModel.Applications.FirstOrDefault(x => x.Config.AppId == _lastConfig.AppId);
            if (newConfig != null)
                _mainPageViewModel.SwitchToApplication(newConfig);

            _mainPageViewModel.ApplicationsChanged -= ApplicationsChanged;
        }

        public event EventHandler CanExecuteChanged = (sender, args) => { };

        /// <summary>
        /// Checks the ID if it already exists and modifies the ID if it does so.
        /// </summary>
        private void UpdateIdIfDuplicate(IApplicationConfig config)
        {
            // Ensure no duplication of AppId
            while (_mainPageViewModel.Applications.Any(x => x.Config.AppId == config.AppId))
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

            if ((bool)dialog.ShowDialog())
            {
                return dialog.FileName;
            }

            return "";
        }
    }
}
