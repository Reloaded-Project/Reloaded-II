namespace Reloaded.Mod.Launcher.Lib.Commands.Download;

/// <summary>
/// This commands allows you to configure NuGet sources for the application.
/// </summary>
public class ConfigureNuGetSourcesCommand : ICommand
{
    private Action _callback;

    /// <summary/>
    /// <param name="callback">Function to execute after NuGet Feeds have been configured.</param>
    public ConfigureNuGetSourcesCommand(Action callback)
    {
        _callback = callback;
    }

    /// <inheritdoc />
    public bool CanExecute(object? parameter) => true;

    /// <inheritdoc />
    public void Execute(object? parameter)
    {
        Actions.ConfigureNuGetFeeds(new ConfigureNuGetFeedsDialogViewModel(IoC.Get<LoaderConfig>()));
        _callback?.Invoke();
    }

    /// <inheritdoc />
    public event EventHandler? CanExecuteChanged;
}