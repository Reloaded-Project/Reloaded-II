# Packaging Library

!!! info

    Library for creating downloadable packages and updates for Reloaded II mods.  
    [[NuGet Package]](https://www.nuget.org/packages/Reloaded.Mod.Loader.Update.Packaging)

## Create Packages

!!! info

    Shows how to create an update package.  

```csharp
// Use the Publisher.PublishAsync API
await Publisher.PublishAsync(new PublishArgs()
{
    ModTuple = new PathTuple<ModConfig>(configPath, config),
    OutputFolder = options.OutputFolder,
    IncludeRegexes = options.IncludeRegexes.ToList(),
    IgnoreRegexes = options.IgnoreRegexes.ToList(),
    OlderVersionFolders = options.OlderVersionFolders.ToList(),
    AutomaticDelta = options.AutomaticDelta,
    CompressionLevel = options.CompressionLevel,
    CompressionMethod = options.CompressionMethod,
    Progress = progressBar.AsProgress<double>(),
    PackageName = options.PackageName,
    PublishTarget = options.PublishTarget,
    ChangelogPath = options.ChangelogPath,
    MetadataFileName = config.ReleaseMetadataFileName
});
```

The parameters are functionally identical to those in the `Reloaded.Publisher` CLI tool.  
Refer to parameters' documentation for definitions.  