using static Reloaded.Mod.Loader.Update.Packaging.Publisher;
using CompressionLevel = SevenZip.CompressionLevel;
using IOEx = Reloaded.Mod.Loader.IO.Utility.IOEx;
using Paths = Sewer56.DeltaPatchGenerator.Lib.Utility.Paths;

namespace Reloaded.Mod.Launcher.Lib.Models.ViewModel.Dialog;

/// <summary>
/// ViewModel used to publish a singular mod to the masses.
/// </summary>
public class PublishModDialogViewModel : ObservableObject
{
    private PathTuple<ModConfig> _modTuple;

    /// <summary>
    /// Target for publishing the package to.
    /// </summary>
    public PublishTarget PublishTarget { get; set; } = PublishTarget.Default;

    /// <summary>
    /// The folder where the published mod should be saved.
    /// </summary>
    public string OutputFolder { get; set; } 

    /// <summary>
    /// Paths to older versions of the same mods.
    /// </summary>
    public ObservableCollection<StringWrapper> OlderVersionFolders { get; set; } = new ObservableCollection<StringWrapper>();

    /// <summary>
    /// The currently selected <see cref="OlderVersionFolders"/> item.
    /// </summary>
    public StringWrapper? SelectedOlderVersionFolder { get; set; } = null;

    /// <summary>
    /// Regexes of files to make sure are ignored.
    /// </summary>
    public ObservableCollection<StringWrapper> IgnoreRegexes { get; set; }

    /// <summary>
    /// The currently selected <see cref="IgnoreRegexes"/> item.
    /// </summary>
    public StringWrapper? SelectedIgnoreRegex { get; set; } = null;

    /// <summary>
    /// Name of the generated 7z file.
    /// </summary>
    public string? PackageName { get; set; } = null;

    /// <summary>
    /// Regexes of files to make sure are not ignored.
    /// </summary>
    public ObservableCollection<StringWrapper> IncludeRegexes { get; set; }

    /// <summary>
    /// The currently selected <see cref="IncludeRegexes"/> item.
    /// </summary>
    public StringWrapper? SelectedIncludeRegex { get; set; } = null;

    /// <summary>
    /// The progress of the build operation.
    /// </summary>
    public double BuildProgress { get; set; }

    /// <summary>
    /// True if currently building, else false.
    /// </summary>
    public bool CanBuild { get; set; } = true;

    /// <summary>
    /// True to show last version's UI items, else false.
    /// </summary>
    public bool ShowLastVersionUiItems { get; set; } = true;

    /// <summary>
    /// The amount by which the mod should be compressed.
    /// </summary>
    public CompressionLevel CompressionLevel { get; set; } = CompressionLevel.Ultra;

    /// <summary>
    /// The method with which the archive should be compressed by.
    /// </summary>
    public CompressionMethod CompressionMethod { get; set; } = CompressionMethod.Lzma2;

    /// <summary>
    /// Automatically creates a delta package when building a new release.
    /// </summary>
    public bool AutomaticDelta { get; set; }

    /// <summary>
    /// Path to the changelog file.
    /// </summary>
    public string? ChangelogPath { get; set; }

    /// <summary>
    /// Path to the readme for the file.
    /// </summary>
    public string? ReadmePath { get; set; }

    /// <summary/>
    public PublishModDialogViewModel(PathTuple<ModConfig> modTuple)
    {
        _modTuple    = modTuple;
        PackageName  = IOEx.ForceValidFilePath(_modTuple.Config.ModName.Replace(' ', '_'));
        OutputFolder = Path.Combine(Path.GetTempPath(), $"{IOEx.ForceValidFilePath(_modTuple.Config.ModId)}.Publish");

        // Set default Regexes.
        IgnoreRegexes = new ObservableCollection<StringWrapper>()
        {
            @".*\.json", // Config files
            $"{Regex.Escape($@"{_modTuple.Config.ModId}.nuspec")}"
        };

        IncludeRegexes = new ObservableCollection<StringWrapper>()
        {
            Regex.Escape(ModConfig.ConfigFileName),
            @"\.deps\.json",
            @"\.runtimeconfig\.json",
        };

        // Set notifications
        PropertyChanged += ChangeUiVisbilityOnPropertyChanged;
    }

    /// <summary>
    /// Builds a new release.
    /// </summary>
    /// <param name="cancellationToken">Used to cancel the current build.</param>
    /// <returns>True if a build has started and the operation completed, else false.</returns>
    public async Task<bool> BuildAsync(CancellationToken cancellationToken = default)
    {
        // Check if Auto Delta can be performed.
        if (AutomaticDelta && !Singleton<ReleaseMetadata>.Instance.CanReadFromDirectory(OutputFolder, null, out _, out _))
        {
            var description = string.Format(Resources.PublishAutoDeltaWarningDescriptionFormat.Get(), Singleton<ReleaseMetadata>.Instance.GetDefaultFileName());
            Actions.DisplayMessagebox(Resources.PublishAutoDeltaWarningTitle.Get(), description);
            return false;
        }

        // Perform Build
        CanBuild = false;
        try
        {
            await Task.Run(async () =>
            {
                await PublishAsync(new PublishArgs()
                {
                    PublishTarget = PublishTarget,
                    OutputFolder = OutputFolder,
                    ModTuple = _modTuple,
                    IgnoreRegexes = IgnoreRegexes.Select(x => x.Value).ToList(),
                    IncludeRegexes = IncludeRegexes.Select(x => x.Value).ToList(),
                    Progress = new Progress<double>(d => BuildProgress = d * 100),
                    AutomaticDelta = AutomaticDelta,
                    CompressionLevel = CompressionLevel,
                    CompressionMethod = CompressionMethod,
                    OlderVersionFolders = OlderVersionFolders.Select(x => x.Value).ToList(),
                    PackageName = PackageName,
                    MetadataFileName = _modTuple.Config.ReleaseMetadataFileName,
                    ChangelogPath = ChangelogPath,
                    ReadmePath = ReadmePath
                });
            });

            ProcessExtensions.OpenFileWithExplorer(OutputFolder);
        }
        catch (Exception ex)
        {
            Errors.HandleException(ex);
            return false;
        }
        finally
        {
            CanBuild = true;
        }

        return true;
    }

    /// <summary>
    /// Adds a folder for another version of the mod.
    /// </summary>
    public void AddNewVersionFolder()
    {
        var configFilePath = FileSelectors.SelectModConfigFile();
        if (string.IsNullOrEmpty(configFilePath))
            return;

        // Try parse.
        ModConfig config;
        try { config = IConfig<ModConfig>.FromPath(configFilePath); }
        catch (Exception)
        {
            Actions.DisplayMessagebox(Resources.ErrorInvalidSemanticVersionTitle.Get(), Resources.ErrorInvalidSemanticVersionDescription.Get());
            return;
        }

        // Check ID
        if (config.ModId != _modTuple.Config.ModId)
        {
            Actions.DisplayMessagebox(Resources.ErrorPublishModIdMismatch.Get(), Resources.ErrorPublishModIdDescription.Get());
            return;
        }

        // Check Version
        if (!NuGetVersion.TryParse(config.ModVersion, out var version))
        {
            Actions.DisplayMessagebox(Resources.ErrorInvalidSemanticVersionTitle.Get(), Resources.ErrorInvalidSemanticVersionDescription.Get());
            return;
        }

        // Check Version Value
        if (new NuGetVersion(_modTuple.Config.ModVersion) <= new NuGetVersion(config.ModVersion))
        {
            Actions.DisplayMessagebox(Resources.ErrorNewerModConfigVersionTitle.Get(), Resources.ErrorNewerModConfigVersionDescription.Get());
            return;
        }

        OlderVersionFolders.Add(Path.GetDirectoryName(configFilePath)!);
    }

    /// <summary>
    /// Removes the folder for last known version.
    /// </summary>
    public void RemoveSelectedVersionFolder() => RemoveSelectedOrLastItem(SelectedOlderVersionFolder, OlderVersionFolders);

    /// <summary>
    /// Removes the selected ignore regex.
    /// </summary>
    public void RemoveSelectedIgnoreRegex() => RemoveSelectedOrLastItem(SelectedIgnoreRegex, IgnoreRegexes);

    /// <summary>
    /// Removes the selected include regex.
    /// </summary>
    public void RemoveSelectedIncludeRegex() => RemoveSelectedOrLastItem(SelectedIncludeRegex, IncludeRegexes);

    /// <summary>
    /// Adds a regular expression for ignoring files.
    /// </summary>
    public void AddIgnoreRegex() => IgnoreRegexes.Add("New Ignore Regex");

    /// <summary>
    /// Adds a regular expression for including files.
    /// </summary>
    public void AddIncludeRegex() => IncludeRegexes.Add("New Include Regex");

    /// <summary>
    /// Calculates all files that will be removed from the final archive and displays them to the user.
    /// </summary>
    public void ShowExcludedFiles()
    {
        var compiledIgnoreRegexes  = IgnoreRegexes.Select(x => new Regex(x, RegexOptions.Compiled));
        var compiledIncludeRegexes = IncludeRegexes.Select(x => new Regex(x, RegexOptions.Compiled));

        var allFiles     = Directory.GetFiles(GetModFolder(), "*.*", SearchOption.AllDirectories).Select(x => Paths.GetRelativePath(x, GetModFolder()));
        var ignoredFiles = allFiles.Where(path => path.TryMatchAnyRegex(compiledIgnoreRegexes) && !path.TryMatchAnyRegex(compiledIncludeRegexes)).ToHashSet();
        Actions.DisplayMessagebox.Invoke(Resources.PublishModRegexDialogTitle.Get(), string.Join('\n', ignoredFiles));
    }

    /// <summary>
    /// Sets the output folder for the build operation.
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    public void SetOutputFolder()
    {
        var dialog = new VistaFolderBrowserDialog();
        dialog.Multiselect = false;
        dialog.SelectedPath = OutputFolder;

        if ((bool)dialog.ShowDialog()!)
            OutputFolder = dialog.SelectedPath;
    }

    /// <summary>
    /// Directs user to the page showing them how to publish mods.
    /// </summary>
    public void ShowPublishTutorial() => ProcessExtensions.OpenFileWithDefaultProgram("https://reloaded-project.github.io/Reloaded-II/CreatingRelease");

    /// <summary>
    /// Lets the user select a new changelog path.
    /// </summary>
    public void SetChangelogPath() => ChangelogPath = FileSelectors.SelectMarkdownFile();

    /// <summary>
    /// Lets the user select a new readme path.
    /// </summary>
    public void SetReadmePath() => ReadmePath = FileSelectors.SelectMarkdownFile();

    private string GetModFolder() => Path.GetDirectoryName(_modTuple.Path)!;
    

    private void RemoveSelectedOrLastItem(StringWrapper? item, ObservableCollection<StringWrapper> allItems)
    {
        if (item != null)
            allItems.Remove(item);
        else if (allItems.Count > 0)
            allItems.RemoveAt(allItems.Count - 1);
    }
    
    private void ChangeUiVisbilityOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(PublishTarget))
            ShowLastVersionUiItems = PublishTarget != PublishTarget.NuGet;
    }
}