using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Ookii.Dialogs.Wpf;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Launcher.Models.ViewModel;
using Reloaded.Mod.Loader.IO;
using Reloaded.Mod.Loader.IO.Config;

namespace Reloaded.Mod.Launcher.Commands.AddAppPage
{
    /// <summary>
    /// Comnmand to be used by the <see cref="AddAppPage"/> which allows
    /// for the addition of a new application.
    /// </summary>
    public class AddApplicationCommand : ICommand
    {
        // ReSharper disable InconsistentNaming
        private const string XAML_AddAppExecutableTitle = "AddAppExecutableTitle";
        private const string XAML_AddAppExecutableFilter = "AddAppExecutableFilter";
        // ReSharper restore InconsistentNaming

        private string _lastConfigFileLocation;
        private AddAppViewModel _addAppViewModel;


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
            IApplicationConfig config = new Loader.IO.Config.ApplicationConfig(Path.GetFileName(fileInfo.FileName), fileInfo.ProductName, exePath);

            // Set AppName if empty & Ensure no duplicate ID.
            if (String.IsNullOrEmpty(config.AppName))
                config.AppName = config.AppId;

            UpdateIdIfDuplicate(config);

            // Get paths.
            string applicationConfigDirectory = LoaderConfigReader.ReadConfiguration().ApplicationConfigDirectory;
            string applicationDirectory = Path.Combine(applicationConfigDirectory, config.AppId);
            string applicationConfigFile = Path.Combine(applicationDirectory, Loader.IO.Config.ApplicationConfig.ConfigFileName);

            // Write file to disk.
            Directory.CreateDirectory(applicationDirectory);
            Loader.IO.Config.ApplicationConfig.WriteConfiguration(applicationConfigFile, (Loader.IO.Config.ApplicationConfig)config);

            // Select this config.
            _lastConfigFileLocation = applicationConfigFile;
            _addAppViewModel.MainPageViewModel.ApplicationsChanged += Handler;
        }

        // Note: New application config notification automatically received by the file system watcher.
        // Select most recent application config.
        async void Handler(object sender, NotifyCollectionChangedEventArgs args)
        {
            if (args.Action != NotifyCollectionChangedAction.Remove)
            {
                // Find index of entry to set.
                var applicationsList = _addAppViewModel.MainPageViewModel.Applications.ToList();
                int index = applicationsList.FindIndex(tuple => tuple.ApplicationConfigPath.Equals(_lastConfigFileLocation));

                // TODO: Remove this hack. Problem is that the ItemsSource property of the combobox is changed as we replace Applications with a new list. This causes the selected index to be reset to 0 and that's no good!
                await Task.Delay(100);
                if (index >= 0)
                    _addAppViewModel.SelectedIndex = index;

                _addAppViewModel.MainPageViewModel.ApplicationsChanged -= Handler;
            }
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
            dialog.Title = (string) Application.Current.Resources[XAML_AddAppExecutableTitle];
            dialog.Filter = $"{(string)Application.Current.Resources[XAML_AddAppExecutableFilter]} (*.exe)|*.exe";
            if ((bool) dialog.ShowDialog())
            {
                return dialog.FileName;
            }

            return "";
        }
    }
}
