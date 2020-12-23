### Create a new .NET Standard Project

![Creating .NET Standard Solution](./Images/ProjectFromScratch1.png)

### Add the `Reloaded.Mod.Interfaces` NuGet package.

![Reloaded Mod Interfaces](./Images/ProjectFromScratch2.png)
![Reloaded Mod Interfaces](./Images/ProjectFromScratch3.png)

### Implement the `IMod` Interface.

![IMod Interface](./Images/ProjectFromScratch4.png)

### Add a generated mod configuration and preview image to the project.

![Generated Mod Configuration](./Images/ProjectFromScratch5.png)
![Generated Mod Configuration](./Images/ProjectFromScratch6.png)
![Generated Mod Configuration](./Images/ProjectFromScratch7.png)

Set their `Build Action` to Content + Copy Always.

### Unload Project and Add Two Properties in .csproj

![Generated Mod Configuration](./Images/ProjectFromScratch8.png)

```xml
<AppendTargetFrameworkToOutputPath>false</AppendTargetFrameworkToOutputPath>
<CopyLocalLockFileAssemblies>true</CopyLocalLockFileAssemblies>
```

