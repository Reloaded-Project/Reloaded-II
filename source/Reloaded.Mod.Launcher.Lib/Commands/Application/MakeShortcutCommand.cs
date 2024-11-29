using Constants = Reloaded.Mod.Launcher.Lib.Misc.Constants;
using Environment = System.Environment;
using File = System.IO.File;

namespace Reloaded.Mod.Launcher.Lib.Commands.Application;

/// <summary>
/// Used to create a shortcut for a Reloaded application.
/// </summary>
public class MakeShortcutCommand : WithCanExecuteChanged, ICommand
{
    private PathTuple<ApplicationConfig>? _config;
    private IIconConverter _iconConverter;

    /// <summary/>
    public MakeShortcutCommand(PathTuple<ApplicationConfig>? config, IIconConverter iconConverter)
    {
        _config = config;
        _iconConverter = iconConverter;
    }

    /* Interface */

    /// <inheritdoc />
    public bool CanExecute(object? parameter)
    {
        if (_config != null)
            return File.Exists(ApplicationConfig.GetAbsoluteAppLocation(_config));

        return false;
    }

    /// <inheritdoc />
    public void Execute(object? parameter)
    {
        var loaderConfig = IoC.Get<LoaderConfig>();
        var shell        = (IShellLink) new ShellLink();

        var appLocation = ApplicationConfig.GetAbsoluteAppLocation(_config);
        shell.SetDescription($"Launch {_config?.Config.AppName} via Reloaded II");
        shell.SetPath($"\"{loaderConfig.LauncherPath}\"");
        shell.SetArguments($"{Constants.ParameterLaunch} \"{appLocation}\"");
        shell.SetWorkingDirectory(Path.GetDirectoryName(loaderConfig.LauncherPath)!);

        if (_config != null)
        {
            var hasIcon = _config.Config.TryGetApplicationIcon(_config.Path, out var logoPath);
            if (hasIcon)
            {
                // Make path for icon.
                string newPath = Path.ChangeExtension(logoPath, ".ico");

                // Convert to ICO.
                _iconConverter.TryConvertToIcon(logoPath, newPath);
                shell.SetIconLocation(newPath, 0);
            }
            else
            {
                shell.SetIconLocation(appLocation, 0);
            }
        }

        // Save the shortcut.
        var file = (IPersistFile) shell;
        var link = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), $"{_config?.Config.AppName.SanitizeFileName()} (Reloaded).lnk");
        file.Save(link, false);

        Actions.DisplayMessagebox?.Invoke(Resources.AddAppShortcutCreatedTitle.Get(), 
            $"{Resources.AddAppShortcutCreatedMessage.Get()} {link}",
            new Actions.DisplayMessageBoxParams() { StartupLocation = Actions.WindowStartupLocation.CenterScreen });
    }
}