using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Versioning;
using Ookii.Dialogs.Wpf;
using Reloaded.Mod.Launcher.Lib.Static;
using Reloaded.Mod.Launcher.Lib.Utility;
using Reloaded.Mod.Loader.IO.Config;
using Reloaded.Mod.Loader.IO.Structs;
using Reloaded.Mod.Loader.IO.Utility;
using SevenZip;
using Sewer56.DeltaPatchGenerator.Lib.Utility;
using Sewer56.Update.Extractors.SevenZipSharp;
using Sewer56.Update.Misc;
using Sewer56.Update.Packaging;
using Sewer56.Update.Packaging.Interfaces;
using Sewer56.Update.Packaging.Structures;
using Sewer56.Update.Packaging.Structures.ReleaseBuilder;
using Sewer56.Update.Resolvers.GameBanana;
using Sewer56.Update.Resolvers.NuGet;
using IOEx = Reloaded.Mod.Loader.IO.Utility.IOEx;

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
    public ObservableCollection<string> OlderVersionFolders { get; set; } = new ObservableCollection<string>();

    /// <summary>
    /// The currently selected <see cref="OlderVersionFolders"/> item.
    /// </summary>
    public string? SelectedOlderVersionFolder { get; set; } = null;

    /// <summary>
    /// Regexes of files to make sure are ignored.
    /// </summary>
    public ObservableCollection<string> IgnoreRegexes { get; set; }

    /// <summary>
    /// The currently selected <see cref="IgnoreRegexes"/> item.
    /// </summary>
    public string? SelectedIgnoreRegex { get; set; } = null;

    /// <summary>
    /// Regexes of files to make sure are not ignored.
    /// </summary>
    public ObservableCollection<string> IncludeRegexes { get; set; }

    /// <summary>
    /// The currently selected <see cref="IncludeRegexes"/> item.
    /// </summary>
    public string? SelectedIncludeRegex { get; set; } = null;

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

    /// <summary/>
    public PublishModDialogViewModel(PathTuple<ModConfig> modTuple)
    {
        _modTuple    = modTuple;
        OutputFolder = Path.Combine(Path.GetTempPath(), $"{IOEx.ForceValidFilePath(_modTuple.Config.ModId)}.Publish");

        // Set default Regexes.
        IgnoreRegexes = new ObservableCollection<string>()
        {
            @".*\.json", // Config files
            Singleton<PackageMetadata>.Instance.GetDefaultFileName().Replace(".", @"\."),
            $@"{_modTuple.Config.ModId}.nuspec".Replace(".", @"\.")
        };

        IncludeRegexes = new ObservableCollection<string>()
        {
            @"ModConfig\.json", // Mod config file.
            @"\.deps\.json",
            @"\.runtimeconfig\.json",
        };

        // Set notifications
        this.PropertyChanged += ChangeUiVisbilityOnPropertyChanged;
    }

    /// <summary>
    /// Builds a new release.
    /// </summary>
    /// <param name="cancellationToken">Used to cancel the current build.</param>
    public async Task BuildAsync(CancellationToken cancellationToken = default)
    {
        CanBuild = false;
        try
        {
            var ignoreRegexesList = IgnoreRegexes.ToList();
            var includeRegexesList = IncludeRegexes.ToList();

            // Arrange
            var builder = new ReleaseBuilder<Empty>();
            builder.AddCopyPackage(new CopyBuilderItem<Empty>()
            {
                FolderPath = GetModFolder(),
                Version = _modTuple.Config.ModVersion,
                IgnoreRegexes = ignoreRegexesList,
                IncludeRegexes = includeRegexesList
            });

            foreach (var olderFolder in OlderVersionFolders)
            {
                var olderPath = Path.Combine(olderFolder, ModConfig.ConfigFileName);
                var config = await IConfig<ModConfig>.FromPathAsync(olderPath, cancellationToken);

                builder.AddDeltaPackage(new DeltaBuilderItem<Empty>()
                {
                    Version = _modTuple.Config.ModVersion,
                    PreviousVersion = config.ModVersion,
                    FolderPath = Path.GetDirectoryName(_modTuple.Path)!,
                    PreviousVersionFolder = olderFolder,
                    IgnoreRegexes = ignoreRegexesList,
                    IncludeRegexes = includeRegexesList
                });
            }

            // Act
            IPackageArchiver archiver = new SevenZipSharpArchiver(new SevenZipSharpArchiverSettings()
            {
                ArchiveFormat = OutArchiveFormat.SevenZip,
                CompressionLevel = CompressionLevel,
                CompressionMethod = CompressionMethod,
                UseFastCompression = false
            });

            if (PublishTarget == PublishTarget.NuGet)
            {
                archiver = new NuGetPackageArchiver(new NuGetPackageArchiverSettings()
                {
                    Id = _modTuple.Config.ModId,
                    Description = _modTuple.Config.ModDescription,
                    Authors = new List<string>() { _modTuple.Config.ModAuthor },
                });
            }

            await Task.Run(() => builder.BuildAsync(new BuildArgs()
            {
                FileName = _modTuple.Config.ModName.Replace(' ', '_'),
                OutputFolder = this.OutputFolder,
                FileNameFilter = GameBananaUtilities.SanitizeFileName,
                PackageArchiver = archiver
            }, new Progress<double>(d =>
            {
                BuildProgress = d * 100;
            })), cancellationToken);

            ProcessExtensions.OpenFileWithExplorer(this.OutputFolder);
        }
        finally
        {
            CanBuild = true;
        }
    }

    /// <summary>
    /// Adds a folder for another version of the mod.
    /// </summary>
    public void AddNewVersionFolder()
    {
        var configFilePath = SelectConfigFile();
        if (string.IsNullOrEmpty(configFilePath))
            return;

        // Try parse.
        ModConfig config;
        try { config = IConfig<ModConfig>.FromPath(configFilePath); }
        catch (Exception e)
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

    private string GetModFolder() => Path.GetDirectoryName(_modTuple.Path)!;
    
    private string SelectConfigFile()
    {
        var dialog = new VistaOpenFileDialog();
        dialog.Title = Resources.PublishSelectConfigTitle.Get();
        dialog.FileName = ModConfig.ConfigFileName;
        dialog.Filter = $"{Resources.PublishSelectConfigFileTypeName.Get()}|ModConfig.json";
        if ((bool)dialog.ShowDialog()!)
            return dialog.FileName;

        return "";
    }

    private void RemoveSelectedOrLastItem(string? item, ObservableCollection<string> allItems)
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

/// <summary>
/// Target to publish the package.
/// </summary>
public enum PublishTarget
{
    /// <summary>
    /// Upload to any non-NuGet site.
    /// </summary>
    Default,

    /// <summary>
    /// Upload package to NuGet.
    /// </summary>
    NuGet
}