using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Reloaded.Mod.Loader.IO.Config;
using Reloaded.Mod.Loader.IO.Structs;
using Reloaded.Mod.Loader.IO.Utility;
using Reloaded.Mod.Loader.Update.Interfaces;
using Reloaded.Mod.Loader.Update.Structures;
using Reloaded.Mod.Loader.Update.Utilities;
using Sewer56.Update.Interfaces;
using Sewer56.Update.Misc;
using Sewer56.Update.Packaging.Interfaces;
using Sewer56.Update.Resolvers;
using Sewer56.Update.Resolvers.NuGet;
using Sewer56.Update.Resolvers.NuGet.Utilities;

namespace Reloaded.Mod.Loader.Update.Providers.NuGet;

/// <summary>
/// Allows for updating packages sourced from NuGet repositories.
/// </summary>
public class NuGetUpdateResolverFactory : IUpdateResolverFactory
{
    /// <summary>
    /// Temporary. For migration to new system.
    /// </summary>
    public static readonly DateTime Now = DateTime.UtcNow;

    private static IPackageExtractor _extractor = new NuGetPackageExtractor();

    /// <inheritdoc />
    public IPackageExtractor Extractor { get; } = _extractor;

    /// <inheritdoc />
    public string ResolverId { get; } = "NuGet";

    /// <inheritdoc />
    public string FriendlyName { get; } = "NuGet Repository";

    /// <inheritdoc/>
    public void Migrate(PathTuple<ModConfig> mod, PathTuple<ModUserConfig>? userConfig)
    {
        var modDirectory   = Path.GetDirectoryName(mod.Path);
        var nuspecFilePath = Path.Combine(modDirectory!, $"{IOEx.ForceValidFilePath(mod.Config.ModId)}.nuspec");
        if (File.Exists(nuspecFilePath))
        {
            this.SetConfiguration(mod, new NuGetConfig()
            {
                AllowUpdateFromAnyRepository = true,
                DefaultRepositoryUrls = new ObservableCollection<StringWrapper>(Singleton<LoaderConfig>.Instance.NuGetFeeds.Select(x => (StringWrapper) (x.URL)))
            });

            mod.Save();
            IOEx.TryDeleteFile(nuspecFilePath);
        }
    }

    /// <inheritdoc/>
    public IPackageResolver? GetResolver(PathTuple<ModConfig> mod, PathTuple<ModUserConfig>? userConfig, UpdaterData data)
    {
        var resolvers = new List<IPackageResolver>();
        var urls = new HashSet<string>();

        // Get all URLs
        if (this.TryGetConfiguration<NuGetConfig>(mod, out var nugetConfig))
        {
            foreach (var url in nugetConfig!.DefaultRepositoryUrls)
                urls.Add(url);

            if (nugetConfig.AllowUpdateFromAnyRepository)
                foreach (var url in data.NuGetFeeds)
                    urls.Add(url);
        }
        else
        {
            // TODO: Remove after some time to complete migration.
            // Only allow update from any source until 4/20.
            if (Now < new DateTime(2022, 04, 20))
            {
                foreach (var url in data.NuGetFeeds)
                    urls.Add(url);
            }
        }

        // Add all resolvers
        foreach (var url in urls)
        {
            resolvers.Add(new NuGetUpdateResolver(
                new NuGetUpdateResolverSettings()
                {
                    AllowUnlisted = true,
                    NugetRepository = new NugetRepository(url),
                    PackageId = mod.Config.ModId
                },
                data.CommonPackageResolverSettings
            ));
        }

        if (resolvers.Count > 0)
            return new AggregatePackageResolver(resolvers);
            
        return null;
    }

    /// <inheritdoc />
    public bool TryGetConfigurationOrDefault(PathTuple<ModConfig> mod, out object configuration)
    {
        var result = this.TryGetConfiguration<NuGetConfig>(mod, out var config);
        configuration = config ?? new NuGetConfig();
        return result;
    }

    /// <summary>
    /// Stores a configuration describing how to update mod using NuGet.
    /// </summary>
    public class NuGetConfig : IConfig<NuGetConfig>
    {
        private const string DefaultCategory = "NuGet Settings";

        /// <summary/>
        [DisplayName("Update from Any Repository")]
        [Category(DefaultCategory)]
        [Description("Allows for this mod to be updated from any NuGet repository.")]
        public bool AllowUpdateFromAnyRepository { get; set; }

        /// <summary/>
        [Category(DefaultCategory)]
        [Description("URL to the NuGet repositories to use to check for updates for this mod.\n" +
                     "Right click to add and remove items.")]
        public ObservableCollection<StringWrapper> DefaultRepositoryUrls { get; set; } = new ObservableCollection<StringWrapper>();
    }
}