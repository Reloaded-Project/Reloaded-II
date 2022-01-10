using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ookii.Dialogs.Wpf;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Launcher.Lib.Models.ViewModel;
using Reloaded.Mod.Launcher.Lib.Models.ViewModel.Dialog;
using Reloaded.Mod.Launcher.Lib.Static;
using Reloaded.Mod.Launcher.Lib.Utility;
using Reloaded.Mod.Loader.Community;
using Reloaded.Mod.Loader.Community.Config;
using Reloaded.Mod.Loader.Community.Utility;
using Reloaded.Mod.Loader.IO.Config;
using Reloaded.Mod.Loader.IO.Services;
using Reloaded.Mod.Loader.IO.Structs;
using Reloaded.Mod.Loader.Update.Interfaces;
using Reloaded.Mod.Loader.Update.Providers.GameBanana;
using Sewer56.Update.Misc;
using Standart.Hash.xxHash;

namespace Reloaded.Mod.Launcher.Lib.Commands.Application;

/// <summary>
/// Command which allows for the addition of a new application configuration.
/// </summary>
public class AddApplicationCommand : ICommand
{
    private readonly MainPageViewModel? _mainPageViewModel;
    private ApplicationConfig? _newConfig;
    private ApplicationConfigService _configService;

    /// <summary/>
    /// <param name="viewModel">Viewmodel of the main page. Used to immediately move launcher to new page. Can be null.</param>
    /// <param name="configService">Provides access to application configurations.</param>
    public AddApplicationCommand(MainPageViewModel? viewModel, ApplicationConfigService configService)
    {
        _mainPageViewModel = viewModel;
        _configService = configService;
    }

    /// <inheritdoc />
    public bool CanExecute(object? parameter) => true;

    /// <summary/>
    /// <param name="parameter">Optional parameter. See <see cref="AddApplicationCommandParams"/>.</param>
    public async void Execute(object? parameter)
    {
        var commandParam = parameter as AddApplicationCommandParams;
        var param = commandParam ?? new AddApplicationCommandParams();

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

        // Work with the index.
        try
        {
            await using var fileStream = new FileStream(config.AppLocation, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 524288);
            var indexApi     = new IndexApi();
            var pathTuple    = new PathTuple<ApplicationConfig>(applicationConfigFile, config);
            var index        = await indexApi.GetIndexAsync();
            var hash         = Hashing.ToString(await xxHash64.ComputeHashAsync(fileStream));
            var applications = index.FindApplication(hash, config.AppId, out bool hashMatches);

            await await ActionWrappers.ExecuteWithApplicationDispatcherAsync(async () =>
            {
                if (applications.Count == 1 && hashMatches)
                {
                    await ApplyIndexEntryAsync(indexApi, applications[0], config, pathTuple, hash);
                }
                else if (applications.Count >= 1)
                {
                    // Select application.
                    var viewModel = new SelectAddedGameDialogViewModel(applications);
                    var result = Actions.ShowSelectAddedGameDialog(viewModel);
                    if (result == null)
                        return;

                    await ApplyIndexEntryAsync(indexApi, applications[0], config, pathTuple, hash);
                }
            });

        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        // Write file to disk.
        Directory.CreateDirectory(applicationDirectory);
        IConfig<ApplicationConfig>.ToPath(config, applicationConfigFile);

        // Listen to event for when the new application is discovered.
        _newConfig = config;
        _configService.Items.CollectionChanged += ApplicationsChanged;

        // Set return value
        param.ResultCreatedApplication = true;
    }

    private static async Task ApplyIndexEntryAsync(IndexApi indexApi, IndexAppEntry applications, ApplicationConfig config, PathTuple<ApplicationConfig> pathTuple, string hash)
    {
        var application = await indexApi.GetApplicationAsync(applications);
        var hashMatches = hash.Equals(application.Hash, StringComparison.OrdinalIgnoreCase);
        if (application.AppStatus == Status.WrongExecutable && hashMatches)
            Actions.DisplayMessagebox(Resources.AddAppRepoBadExecutable.Get(), application.BadStatusDescription!);

        config.AppName = application.AppName;
        // Apply GB Configurations
        Singleton<GameBananaPackageProviderFactory>.Instance.SetConfiguration(pathTuple,
            new GameBananaPackageProviderFactory.GameBananaProviderConfig()
            {
                GameId = (int)application.GameBananaId
            });

        if (!hashMatches)
        {
            var viewModel = new AddAppHashMismatchDialogViewModel(application.BadHashDescription!);
            Actions.ShowAddAppHashMismatchDialog(viewModel);
        }

        if (application.TryGetError(Path.GetDirectoryName(config.AppLocation)!, out var errors))
        {
            var viewModel = new AddApplicationWarningDialogViewModel(errors);
            Actions.ShowApplicationWarningDialog(viewModel);
        }
    }

    private void ApplicationsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        var newConfig = _configService.Items.FirstOrDefault(x => x.Config.AppId == _newConfig?.AppId);
        if (newConfig != null)
            _mainPageViewModel?.SwitchToApplication(newConfig);

        _configService.Items.CollectionChanged -= ApplicationsChanged;
    }

    /// <inheritdoc />
    public event EventHandler? CanExecuteChanged = (_, _) => { };

    /// <summary>
    /// Checks the ID if it already exists and modifies the ID if it does so.
    /// </summary>
    private void UpdateIdIfDuplicate(IApplicationConfig config)
    {
        // Ensure no duplication of AppId
        while (_configService.Items.Any(x => x.Config.AppId == config.AppId))
            config.AppId += "_dup";
    }

    /// <summary>
    /// Opens up a file selection dialog allowing for the selection of an executable to associate with the profile.
    /// </summary>
    private string SelectEXEFile()
    {
        var dialog = new VistaOpenFileDialog();
        dialog.Title = Resources.AddAppExecutableTitle.Get();
        dialog.Filter = $"{Resources.AddAppExecutableFilter.Get()} (*.exe)|*.exe";

        if ((bool)dialog.ShowDialog()!)
            return dialog.FileName;

        return "";
    }
}

/// <summary>
/// Parameters to pass to <see cref="AddApplicationCommand"/>'s <see cref="AddApplicationCommand.Execute"/> function.
/// </summary>
public class AddApplicationCommandParams
{
    /// <summary>
    /// The result of the operation.
    /// True if a new application was created, else false.
    /// </summary>
    public bool ResultCreatedApplication;
}