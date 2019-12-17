using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Windows;
using System.Windows.Input;
using Reloaded.Mod.Launcher.Commands.AddAppPage.Shortcuts;
using Reloaded.Mod.Launcher.Models.ViewModel;
using Reloaded.Mod.Launcher.Commands.Templates;
using Reloaded.Mod.Launcher.Misc;
using Reloaded.Mod.Loader.IO.Config;
using Reloaded.WPF.Utilities;
using File = System.IO.File;
using MessageBox = Reloaded.Mod.Launcher.Pages.Dialogs.MessageBox;
using PathSanitizer = Reloaded.Mod.Shared.PathSanitizer;

namespace Reloaded.Mod.Launcher.Commands.AddAppPage
{
    public class MakeShortcutCommand : WithCanExecuteChanged, ICommand, IDisposable
    {
        private XamlResource<string> _xamlShortcutCreatedTitle = new XamlResource<string>("AddAppShortcutCreatedTitle");
        private XamlResource<string> _xamlShortcutCreatedMessage = new XamlResource<string>("AddAppShortcutCreatedMessage");
        private AddAppViewModel _addAppViewModel;
        private ApplicationConfig _lastConfig;

        public MakeShortcutCommand()
        {
            _addAppViewModel = IoC.Get<AddAppViewModel>();
            _addAppViewModel.PropertyChanged += AddAppViewModelOnPropertyChanged;

            if (_addAppViewModel.Application != null)
            {
                _lastConfig = _addAppViewModel.Application.Config;
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
                    _lastConfig = _addAppViewModel.Application.Config;
                    _lastConfig.PropertyChanged += ApplicationOnLocationPropertyChanged;
                }

                RaiseCanExecute(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }

        private void ApplicationOnLocationPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(_addAppViewModel.Application.Config.AppLocation))
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
            var applicationTuple = _addAppViewModel.MainPageViewModel.Applications.FirstOrDefault(x => x.Config.Equals(_lastConfig));
            var loaderConfig = IoC.Get<LoaderConfig>();
            var shell        = (IShellLink) new ShellLink();

            shell.SetDescription($"Launch {applicationTuple?.Config.AppName} via Reloaded II");
            shell.SetPath($"\"{loaderConfig.LauncherPath}\"");
            shell.SetArguments($"{Constants.ParameterLaunch} \"{_lastConfig.AppLocation}\"");
            shell.SetWorkingDirectory(Path.GetDirectoryName(loaderConfig.LauncherPath));

            if (applicationTuple != null)
            {
                var hasIcon = ApplicationConfig.TryGetApplicationIcon(applicationTuple.ConfigPath, applicationTuple.Config, out var logoPath);
                if (hasIcon)
                {
                    // Make path for icon.
                    string newPath = Path.ChangeExtension(logoPath, ".ico");

                    // Convert to ICO and save.
                    var bitmapImage = Imaging.BitmapFromUri(new Uri(logoPath, UriKind.Absolute));
                    var bitmap = Imaging.BitmapImageToBitmap(bitmapImage);
                    var resizedBitmap = Imaging.ResizeImage(bitmap, Constants.IcoMaxWidth, Constants.IcoMaxHeight);

                    using (var newIcon = Icon.FromHandle(resizedBitmap.GetHicon()))
                    using (Stream newIconStream = new FileStream(newPath, FileMode.Create))
                    {
                        newIcon.Save(newIconStream);
                    }

                    shell.SetIconLocation(newPath, 0);
                }
                else
                {
                    shell.SetIconLocation(_lastConfig.AppLocation, 0);
                }
            }

            // Save the shortcut.
            var file = (IPersistFile) shell;
            var link = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), $"{applicationTuple?.Config.AppName} via Reloaded II.lnk");
            file.Save(link, false);

            var messageBox = new MessageBox(_xamlShortcutCreatedTitle.Get(),
                                            $"{_xamlShortcutCreatedMessage.Get()} {link}");
            messageBox.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            messageBox.ShowDialog();
        }
    }
}
