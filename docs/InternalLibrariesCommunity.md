# Community Index Library

!!! info

    API for [Reloaded II's Community Index](https://github.com/Reloaded-Project/Reloaded.Community) which provides compatibility and user suggestions for games.  
    [[NuGet Package]](https://www.nuget.org/packages/Reloaded.Mod.Loader.Community)  

## Index API

!!! info

    Shows how to fetch data from the community index.  

```csharp
// Can specify optional parameter to fetch index from alternative URL or from filesystem.
// Get the Index
var indexApi  = new IndexApi();
var index = await indexApi.GetIndexAsync();
```

### Get Application in Index

!!! info

    Shows how to find an application within the index.

```csharp
// exeLocation contains the absolute file path to an EXE.
await using var fileStream = new FileStream(exeLocation, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 524288);
var hash = Hashing.ToString(await Hashing.FromStreamAsync(fileStream));

// index variable taken from previous example.
// appId is the name of the EXE, in lower case, e.g. tsonic_win.exe  [Path.GetFileName(filePath).ToLower()]
var applications = index.FindApplication(hash, appId, out bool hashMatches);

if (applications.Count == 1 && hashMatches)
{
    if (hashMatches) 
    {
        // Guaranteed match! Get the application info.
        var application = await indexApi.GetApplicationAsync(applications[0]);

        // use TryGetError to validate game data against possible incompatibilities.
    }
    else 
    {
        // Hash does not match.
        // Either wrong game, or EXE was modified by user.
        // Must be resolved by user.
    }
}
else if (applications.Count > 1) 
{
    // Multiple matches, must be resolved by user.
    // Rare!
}
```