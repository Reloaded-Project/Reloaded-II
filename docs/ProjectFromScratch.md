# Notice

This guide is a bit dated, however should still be relevant to modern Reloaded versions.

## Create a new .NET Project

![Creating .NET Standard Solution](./Images/ProjectFromScratch1.png)

Recommended to target the same version of .NET as used by the loader.  
*Do not* use a version of .NET newer than the loader.  

## Add the `Reloaded.Mod.Interfaces` NuGet package.

![Reloaded Mod Interfaces](./Images/ProjectFromScratch2.png)
![Reloaded Mod Interfaces](./Images/ProjectFromScratch3.png)

## Implement the `IMod` Interface.

![IMod Interface](./Images/ProjectFromScratch4.png)

## Create A Blank Mod in Reloaded. Copy Configuration & Preview Image.

![Generated Mod Configuration](./Images/ProjectFromScratch5.png)
![Generated Mod Configuration](./Images/ProjectFromScratch6.png)
![Generated Mod Configuration](./Images/ProjectFromScratch7.png)

Set their `Build Action` to Content + Copy Always.

## Unload Project and Add Two Properties in .csproj

![Generated Mod Configuration](./Images/ProjectFromScratch8.png)

```xml
<AppendTargetFrameworkToOutputPath>false</AppendTargetFrameworkToOutputPath>
<CopyLocalLockFileAssemblies>true</CopyLocalLockFileAssemblies>
```

