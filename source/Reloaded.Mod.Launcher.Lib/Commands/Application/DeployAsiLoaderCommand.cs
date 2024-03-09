namespace Reloaded.Mod.Launcher.Lib.Commands.Application;

/// <summary>
/// This command allows you to deploy Ultimate ASI Loader to an application.
/// </summary>
public class DeployAsiLoaderCommand : WithCanExecuteChanged, ICommand
{
    private AsiLoaderDeployer _deployer;

    /// <inheritdoc />
    public DeployAsiLoaderCommand(PathTuple<ApplicationConfig> applicationTuple)
    {
        _deployer = new AsiLoaderDeployer(applicationTuple);
    }
    
    /* Implementation */

    /// <inheritdoc />
    public bool CanExecute(object? parameter) => _deployer.CanDeploy();

    /// <inheritdoc />
    public void Execute(object? parameter)
    {
        var deployConfirm = Actions.DisplayMessagebox.Invoke(Resources.AsiLoaderDialogTitle.Get(), Resources.AsiLoaderDialogDescription.Get(), new Actions.DisplayMessageBoxParams()
        {
            StartupLocation = Actions.WindowStartupLocation.CenterScreen,
            Type = Actions.MessageBoxType.OkCancel
        });

        if (!deployConfirm) 
            return;

        _deployer.DeployAsiLoader(out string? loaderPath, out string bootstrapperPath);
        PrintDeployedAsiLoaderInfo(bootstrapperPath, loaderPath);
    }

    internal static void PrintDeployedAsiLoaderInfo(string bootstrapperPath, string? loaderPath)
    {
        var deployedBootstrapper = $"{Resources.AsiLoaderDialogBootstrapperDeployed.Get()} {bootstrapperPath}";
        if (loaderPath == null)
        {
            // Installed Bootstrapper but not loader.
            Actions.DisplayMessagebox.Invoke(Resources.AsiLoaderDialogTitle.Get(), deployedBootstrapper);
        }
        else
        {
            var deployedLoader = $"{Resources.AsiLoaderDialogLoaderDeployed.Get()} {loaderPath}";
            Actions.DisplayMessagebox.Invoke(Resources.AsiLoaderDialogTitle.Get(), $"{deployedLoader}\n{deployedBootstrapper}");
        }
    }

    /// <summary>
    /// Raises the property changed event.
    /// </summary>
    public void RaisePropertyChanged() => RaiseCanExecute(null!, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
}