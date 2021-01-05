using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using System.Windows;
using System.Windows.Input;
using Reloaded.Mod.Launcher.Commands.EditAppPage.Shortcuts;
using Reloaded.Mod.Launcher.Commands.Templates;
using Reloaded.Mod.Launcher.Misc;
using Reloaded.Mod.Loader.IO.Config;
using Reloaded.Mod.Loader.IO.Structs;
using Reloaded.WPF.Utilities;
using File = System.IO.File;
using MessageBox = Reloaded.Mod.Launcher.Pages.Dialogs.MessageBox;

namespace Reloaded.Mod.Launcher.Commands.ApplicationPage
{
    public class MakeShortcutCommand : WithCanExecuteChanged, ICommand
    {
        private XamlResource<string> _xamlShortcutCreatedTitle = new XamlResource<string>("AddAppShortcutCreatedTitle");
        private XamlResource<string> _xamlShortcutCreatedMessage = new XamlResource<string>("AddAppShortcutCreatedMessage");
        private PathTuple<ApplicationConfig> _config;

        public MakeShortcutCommand(PathTuple<ApplicationConfig> config)
        {
            _config = config;
        }

        /* Interface */
        public bool CanExecute(object parameter)
        {
            if (_config != null)
                return File.Exists(_config.Config.AppLocation);

            return false;
        }

        public void Execute(object parameter)
        {
            var loaderConfig = IoC.Get<LoaderConfig>();
            var shell        = (IShellLink) new ShellLink();

            shell.SetDescription($"Launch {_config?.Config.AppName} via Reloaded II");
            shell.SetPath($"\"{loaderConfig.LauncherPath}\"");
            shell.SetArguments($"{Constants.ParameterLaunch} \"{_config.Config.AppLocation}\"");
            shell.SetWorkingDirectory(Path.GetDirectoryName(loaderConfig.LauncherPath));

            if (_config != null)
            {
                var hasIcon = _config.Config.TryGetApplicationIcon(_config.Path, out var logoPath);
                if (hasIcon)
                {
                    // Make path for icon.
                    string newPath = Path.ChangeExtension(logoPath, ".ico");

                    // Convert to ICO.
                    Imaging.TryConvertToIcon(logoPath, newPath);
                    shell.SetIconLocation(newPath, 0);
                }
                else
                {
                    shell.SetIconLocation(_config.Config.AppLocation, 0);
                }
            }

            // Save the shortcut.
            var file = (IPersistFile) shell;
            var link = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), $"{_config?.Config.AppName} (Reloaded).lnk");
            file.Save(link, false);

            var messageBox = new MessageBox(_xamlShortcutCreatedTitle.Get(),
                                            $"{_xamlShortcutCreatedMessage.Get()} {link}");
            messageBox.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            messageBox.ShowDialog();
        }
    }
}
