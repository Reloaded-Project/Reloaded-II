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
using Reloaded.Mod.Loader.IO.Services;
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
        private ApplicationConfig _newConfig;
        private ApplicationConfigService _configService;

        public AddApplicationCommand(MainPageViewModel viewModel, ApplicationConfigService configService)
        {
            _mainPageViewModel = viewModel;
            _configService = configService;
        }

        public bool CanExecute(object parameter) => true;
        
        /// <param name="parameter">Optional parameter. Returns true if user created an application, else false.</param>
        public void Execute(object parameter)
        {
            var param = parameter == null ? new AddApplicationCommandParams() : parameter as AddApplicationCommandParams;

            // Select EXE
            string exePath = SelectEXEFile();

            if (string.IsNullOrEmpty(exePath) || !File.Exists(exePath))
            {
                param.ResultCreatedApplication = false;
                return;
            }

            // Get file information and populate initial application details.
            var fileInfo = FileVersionInfo.GetVersionInfo(exePath);
            var config = new ApplicationConfig(Path.GetFileName(fileInfo.FileName).ToLower(), fileInfo.ProductName, exePath);

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
            IConfig<ApplicationConfig>.ToPath(config, applicationConfigFile);

            // Listen to event for when the new application is discovered.
            _newConfig = (ApplicationConfig)config;
            _configService.Items.CollectionChanged += ApplicationsChanged;

            // Set return value
            param.ResultCreatedApplication = true;
        }

        private void ApplicationsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var newConfig = _configService.Items.FirstOrDefault(x => x.Config.AppId == _newConfig.AppId);
            if (newConfig != null)
                _mainPageViewModel.SwitchToApplication(newConfig);

            _configService.Items.CollectionChanged -= ApplicationsChanged;
        }

        public event EventHandler CanExecuteChanged = (sender, args) => { };

        /// <summary>
        /// Checks the ID if it already exists and modifies the ID if it does so.
        /// </summary>
        private void UpdateIdIfDuplicate(IApplicationConfig config)
        {
            // Ensure no duplication of AppId
            while (_configService.Items.Any(x => x.Config.AppId == config.AppId))
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

        public class AddApplicationCommandParams
        {
            public bool ResultCreatedApplication;
        }
    }
}
