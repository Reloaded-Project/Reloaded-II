using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Reloaded.Mod.Launcher.Models.ViewModel;
using IWshRuntimeLibrary;
using Reloaded.Mod.Launcher.Commands.Templates;
using Reloaded.Mod.Launcher.Misc;
using Reloaded.Mod.Loader.IO.Config;
using File = System.IO.File;

namespace Reloaded.Mod.Launcher.Commands.AddAppPage
{
    public class MakeShortcutCommand : WithCanExecuteChanged, ICommand, IDisposable
    {
        private AddAppViewModel _addAppViewModel;
        private ApplicationConfig _lastConfig;

        public MakeShortcutCommand()
        {
            _addAppViewModel = IoC.Get<AddAppViewModel>();
            _addAppViewModel.PropertyChanged += AddAppViewModelOnPropertyChanged;

            if (_addAppViewModel.Application != null)
            {
                _lastConfig = _addAppViewModel.Application;
                _lastConfig.PropertyChanged += ApplicationOnLocationPropertyChanged;
            }
        }

        ~MakeShortcutCommand()
        {
            Dispose();
        }

        public void Dispose()
        {
            _addAppViewModel.PropertyChanged -= AddAppViewModelOnPropertyChanged;
            _lastConfig.PropertyChanged -= ApplicationOnLocationPropertyChanged;
            GC.SuppressFinalize(this);
        }

        /* Implementation */
        private void AddAppViewModelOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(_addAppViewModel.Application))
            {
                // Unsubscribe old and resubscribe new.
                if (_lastConfig != null)
                    _lastConfig.PropertyChanged -= ApplicationOnLocationPropertyChanged;

                if (_addAppViewModel.Application != null)
                {
                    _lastConfig = _addAppViewModel.Application;
                    _lastConfig.PropertyChanged += ApplicationOnLocationPropertyChanged;
                }

                RaiseCanExecute(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }

        private void ApplicationOnLocationPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(_addAppViewModel.Application.AppLocation))
                RaiseCanExecute(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        /* Interface */
        public bool CanExecute(object parameter)
        {
            if (_lastConfig != null)
                return File.Exists(_lastConfig.AppLocation);

            return false;
        }

        public void Execute(object parameter)
        {
            string shortcutName = $"{PathSanitizer.ForceValidFilePath($"Reloaded!{_lastConfig.AppName}")}.lnk";
            string link = $"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\\{shortcutName}";

            var shell       = new WshShell();
            var shortcut    = shell.CreateShortcut(link) as IWshShortcut;

            var loaderConfig = IoC.Get<LoaderConfig>();
            shortcut.TargetPath = $"\"{loaderConfig.LauncherPath}\"";
            shortcut.Arguments = $"{Constants.ParameterLaunch} \"{_lastConfig.AppLocation}\"";
            shortcut.WorkingDirectory = Path.GetDirectoryName(loaderConfig.LauncherPath);
            shortcut.Save();
        }
    }
}
