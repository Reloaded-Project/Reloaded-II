# Update Library

!!! info

    Library for resolving dependencies, searching and downloading Reloaded II mod updates.  
    [[NuGet Package]](https://www.nuget.org/packages/Reloaded.Mod.Loader.Update)

## Check & Apply Mod Updates

!!! info

    Checks and applies updates for Reloaded mods.  

```csharp
// modConfigService: See IO Library
// modUserConfigService: See IO Library
var updater       = new Updater(modConfigService, modUserConfigService, updaterData);
var updateDetails = await updater.GetUpdateDetailsAsync();

if (updateDetails.HasUpdates())
{
    // Warning: Consider checking if files are in use.  
    // Otherwise might be stuck waiting for a very long time, there is no timeout.  
    await Updater.Update(Summary);
}
```

Alternative lower level API: `PackageResolverFactory`.  

## Search for Mods

!!! info

    Shows how to search for mods (NuGet, GameBanana, other supported providers).  

```csharp
// May return null if none are available.
// appConfig represents ApplicationConfig from IO.
var aggregateProvider = PackageProviderFactory.GetProvider(appConfig);

// Search for 'Cool Mod' return, 20 entries, skip 0.
var results = await aggregateProvider.SearchAsync("Cool Mod", 0, 20);
```

To search for NuGet packages, you will need to manually create `NuGetPackageProvider`:  

```csharp
// loaderConfig is Mod Loader config
var nuGetRepository = new AggregateNugetRepository(loaderConfig.NuGetFeeds);
var nuGetProvider = new NuGetPackageProvider(nuGetRepository);
```

You can use `AggregatePackageProvider` to combine multiple providers:  

```csharp
// Extract from existing provider.
var provider = new AggregatePackageProvider(new IDownloadablePackageProvider[] { nuGetProvider, aggregateProvider }, "NuGet");
```

## Resolve Missing Dependencies For Mod

!!! info

    Shows how to check and download missing dependencies for mods.

Download dependencies for a mod:  
```csharp
// nuGetRepository is AggregateNugetRepository
// mod is ModConfig
var resolver = DependencyResolverFactory.GetInstance(nuGetRepository);
var result = await resolver.ResolveAsync(mod.ModId, mod.PluginData);
```

For multiple mods:  
```csharp
// The loop is used to resolve nested dependencies (dependencies of dependencies).  
// Non-NuGet sources usually do not have the ability to resolve those.  
ModDependencyResolveResult resolveResult = null!;

do
{
    var missingDeps = modConfigService.GetMissingDependencies();
    var resolver = DependencyResolverFactory.GetInstance(nuGetRepository);

    var results = new List<Task<ModDependencyResolveResult>>();
    foreach (var dependencyItem in missingDeps.Items)
    foreach (var dependency in dependencyItem.Dependencies)
        results.Add(resolver.ResolveAsync(dependency, dependencyItem.Mod.PluginData, token));

    await Task.WhenAll(results); // wait for completion
    resolveResult = ModDependencyResolveResult.Combine(results.Select(x => x.Result)); // merge results

    // Download dependencies here using resolveResult.FoundDependencies
    // so on next loop, will find less dependencies.
} 
while (resolveResult.FoundDependencies.Count > 0);
```

## Write Dependency Metadata

!!! info

    Modifies mod configurations to insert data required for resolving dependencies.  

    e.g. If Mod A depends on Mod B, which receives updates from GitHub, Mod A's config will be modified to include Mod B's update configuration. This will allow people to download Mod B if they have Mod A.  

```csharp
// modConfigService is ModConfigService
await DependencyMetadataWriterFactory.ExecuteAllAsync(modConfigService);
```

## Check Reloaded Dependencies

!!! info

    Shows how to check if all runtimes required to run Reloaded are available.  

```csharp
// loaderConfig is LoaderConfig from IO library.
var deps = new DependencyChecker(loaderConfig, IntPtr.Size == 8); // 64-bit check
if (deps.AllAvailable) 
    return;

// In deps array you can get install urls for any missing runtimes via the interface.
```

Grabs loader path from the loader config. This is used to check for missing dependencies on boot in cases where e.g. user installed 64-bit runtime but not 32-bit after ignoring the installer.