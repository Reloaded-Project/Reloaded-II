<div align="center">
	<h1>Reloaded II: Ready To Run Guide</h1>
	<img src="./Images/Reloaded/Reloaded Logo.png" width="150" align="center" />
	<br/> <br/>
	<strong>User Guides?</strong>
	<br/>
    Imagine being Todd Rogers
    <br/>
    And starting in SECOND GEAR.
</div>


# ReadyToRun

.NET Core 3 has a new type of officially supported file format for publishing applications known as *ReadyToRun* (abbreviated as R2R).

The main advantage of R2R is that it boasts **significant** improvement to startup times by shipping native code alongside IL code to essentially create hybrid assemblies.

As for disadvantages, assembly size is noticeably increased and in the context of Reloaded, you now require separate assemblies for each architecture ('AnyCPU' is not supported).

You can read more about R2R in the following web resources:

- [Design Document for ReadyToRun](https://github.com/dotnet/coreclr/blob/master/Documentation/botr/readytorun-overview.md)
- [What's new in .NET Core 3: ReadyToRun Images](https://docs.microsoft.com/en-us/dotnet/core/whats-new/dotnet-core-3-0#readytorun-images)
- [Announcing .NET Core 3 Preview 6](https://devblogs.microsoft.com/dotnet/announcing-net-core-3-0-preview-6/)

### How to Build Mods as ReadyToRun

At the time of writing, R2R is only officially supported for applications, not class libraries like Reloaded DLL mods. However... well... don't let someone tell you that you can't do it. You can run for president if you want :-).

A .NET Assembly is a .NET Assembly, nobody says you can't use it just because it wasn't compiled with the intention of being used as a *Class Library*.

##### Step 1: Upgrade Project to .NET Core 3

Inside your mod project's `.csproj`, change the version from `netstandard2.0` (or current default) to `netcoreapp3.0` (or newer).

```xml
<TargetFramework>netcoreapp3.0</TargetFramework>
```

##### Step 2: Change Application Output Type

As R2R is currently disabled for Class Libraries, the next best alternative is a "Windows Application". Add to your `.csproj` (or change in Visual Studio project properties).

```xml
<ApplicationIcon />
<OutputType>WinExe</OutputType>
<StartupObject />
```

##### Step 3: Add a dummy entry point.
  Just to shut up the compiler add a dummy entry point that does nothing anywhere in your mod.

```csharp
public static void Main() { }
```

Now you can compile in the Ready2Run format.
Open a command prompt, change directory to project directory and try the following command:

`dotnet publish YourSolution.sln -c Release -r win-x64 --self-contained false -o "ReadyToRunTest" /p:PublishReadyToRun=true`

This should output in a directory called "ReadyToRunTest", with all of the assemblies in the R2R format. You'll notice the file sizes are much bigger than usual. There will also be an `.exe` file, you can delete it if you want.

### Using ReadyToRun Mods with Reloaded II

In order to use R2R mods with Reloaded II, the only real change you need to do is to edit your `ModConfig.json` file.

Inside `ModConfig.json` there are two fields allowing you to specify the 32-bit and 64-bit R2R DLLs to use (for x86 and x64 applications respectively). These are named `ModR2RManagedDll32` and `ModR2RManagedDll64` accordingly.

Example:

```json
"ModDll": "Reloaded.Hooks.ReloadedII.dll",
"ModR2RManagedDll32": "x86/Reloaded.Hooks.ReloadedII.dll",
"ModR2RManagedDll64": "x64/Reloaded.Hooks.ReloadedII.dll",
```

The mod loader will use the optimized R2R assemblies provided the following conditions are true:

- `ModR2RManagedDll32`/`ModR2RManagedDll64` is not empty.
- The file listed by `ModR2RManagedDll32`/`ModR2RManagedDll64` exists.

Otherwise it will fall back to the standard `ModDll` entry.

Reloaded's launcher by default is built in x64 mode, so if you wish to support in-launcher mod configuration, **you must provide a 64-bit R2R build**.

##### Using ReadyToRun In Practice

Using the command prompt to build R2R assemblies every time for testing sounds very cumbersome. Thankfully, you don't have to do that.

In practice, you develop using standard `AnyCPU` builds in your preferred IDE, and only deploy the R2R packed mods to the end users. You take advantage of the `ModDll` fallback mentioned above, supplying both the `ModDll` and the R2R Dll entries in `ModConfig.json`.

To build the individual mods as R2R, I personally use PowerShell, with modified versions of the following simple script:

```powershell
# Project Output Paths
$modOutputPath = "Release"
$solutionName = "Reloaded.Hooks.ReloadedII.sln"
$publishName = "reloaded.sharedlib.hooks.zip"
$publishDirectory = "Publish"

[Environment]::CurrentDirectory = $PWD

# Clean Release and Publish Directories
Remove-Item $modOutputPath -Recurse
Remove-Item $publishDirectory -Recurse
New-Item $modOutputPath -ItemType Directory
New-Item $publishDirectory -ItemType Directory

# Build
dotnet restore $solutionName
dotnet clean $solutionName
dotnet publish $solutionName -c Release -r win-x86 --self-contained false -o "$modOutputPath/x86" /p:PublishReadyToRun=true
dotnet publish $solutionName -c Release -r win-x64 --self-contained false -o "$modOutputPath/x64" /p:PublishReadyToRun=true

# Remove Redundant Files
Remove-Item "$modOutputPath/x86/Preview.png"
Remove-Item "$modOutputPath/x64/Preview.png"
Remove-Item "$modOutputPath/x86/ModConfig.json"
Remove-Item "$modOutputPath/x64/ModConfig.json"

# Remove Unnecessary Debug and Documentation files for Deployment
Get-ChildItem $modOutputPath -Include *.pdb -Recurse | Remove-Item -Force -Recurse
Get-ChildItem $modOutputPath -Include *.xml -Recurse | Remove-Item -Force -Recurse

# Compress
Add-Type -A System.IO.Compression.FileSystem
[IO.Compression.ZipFile]::CreateFromDirectory($modOutputPath, "$publishDirectory/$publishName")
```

This can be adjusted for any project by editing the first four lines.
By default, it generates a `Release` directory, with the built mod inside and a `Publish` directory, containing a zip file, ready for uploading to the web.

Consider the [Hooks Shared Library](https://github.com/Sewer56/Reloaded.SharedLib.Hooks) as a mod example which is developed in this way.