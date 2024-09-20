[cmdletbinding()]
param (
    ## => User Config <= ## 
    $Version = "1.0.0"
)

# .NET 6 Has Issues with Handles and Files Already in use when building
# single file applications, we have to try work around it here.
function New-TemporaryDirectory 
{
    $parent = [System.IO.Path]::GetTempPath()
    [string] $name = [System.Guid]::NewGuid()
	
	$returnValue = Join-Path "$parent" "$name"
	Write-Host "Location: $returnValue"
	return "$returnValue"
}

# Set Working Directory
Split-Path $MyInvocation.MyCommand.Path | Push-Location
[Environment]::CurrentDirectory = $PWD
Write-Host "New Current Directory: $(Get-Location)"

# Build Locations
$buildPath = New-TemporaryDirectory
$outputPath = "$buildPath/Publish"
$outputPath32 = "$buildPath/Publish/x86"
$toolsPath  = "$buildPath/Tools/"
$loaderOutputPath = "$outputPath/Loader/"
$loader32OutputPath = "$outputPath/Loader/x86"
$loader64OutputPath = "$outputPath/Loader/x64"
$chocoPath = "./Chocolatey"
$chocoToolsPath = "$chocoPath/tools"

# Project Paths
$bootstrapperPath = "Reloaded.Mod.Loader.Bootstrapper/Reloaded.Mod.Bootstrapper.vcxproj"
$installerProjectPath = "./Reloaded.Mod.Installer/Reloaded.Mod.Installer.csproj"
$installerCliProjectPath = "./Reloaded.Mod.Installer.Cli/Reloaded.Mod.Installer.Cli.csproj"
$launcherProjectPath = "Reloaded.Mod.Launcher/Reloaded.Mod.Launcher.csproj"
$loaderProjectPath = "Reloaded.Mod.Loader/Reloaded.Mod.Loader.csproj"
$templateProjectPath = "Reloaded.Mod.Template/Reloaded.Mod.Template.NuGet.csproj"

$communityProjectPath = "Tools/Reloaded.Community.Tool/Reloaded.Community.Tool.csproj"
$nugetConverterProjectPath = "Tools/NugetConverter/NugetConverter.csproj"
$publisherProjectPath = "Tools/Reloaded.Publisher/Reloaded.Publisher.csproj"

# Outputs
$publishDirectory = "Publish"
$chocoPublishDirectory = "$publishDirectory/Chocolatey"
$installerPublishDirectory = "$publishDirectory/Installer"
$installerStaticPublishDirectory = "$publishDirectory/Installer-Static"
$templatePublishDirectory = "$publishDirectory/ModTemplate"
$releaseFolder = "/Release"
$toolsReleaseFileName = "/Tools.zip"
$cleanupPaths = ("$buildPath", "$toolsPath", "$publishDirectory", "$chocoToolsPath")

# Clean output directories
foreach ($cleanupPath in $cleanupPaths) {
    Get-ChildItem "$cleanupPath" -Include * -Recurse -ErrorAction SilentlyContinue | Remove-Item -Force -Recurse -ErrorAction SilentlyContinue
}

Get-ChildItem "$chocoPath" -Include *.nupkg -Recurse -ErrorAction SilentlyContinue | Remove-Item -Force -Recurse

# Build using Visual Studio
msbuild $bootstrapperPath /p:Configuration=Release /p:Platform=x64 /p:OutDir="$loaderOutputPath"

# Build AnyCPU, and then copy 32-bit AppHost. 
dotnet publish "$launcherProjectPath" -c Release --self-contained false -o "$outputPath"
dotnet publish "$launcherProjectPath" -c Release -r win-x86 --self-contained false -o "$outputPath32"

# Build Loader
dotnet publish "$loaderProjectPath" -c Release -r win-x64 --self-contained false -o "$loader64OutputPath" /p:PublishReadyToRun=true
dotnet publish "$loaderProjectPath" -c Release -r win-x86 --self-contained false -o "$loader32OutputPath" /p:PublishReadyToRun=true

# Build Tools
dotnet publish "$nugetConverterProjectPath" -c Release -r win-x64 --self-contained false -o "$toolsPath" /p:PublishSingleFile=true
dotnet publish "$publisherProjectPath" -c Release -r win-x64 --self-contained false -o "$toolsPath" /p:PublishSingleFile=true
dotnet publish "$communityProjectPath" -c Release -r win-x64 --self-contained false -o "$toolsPath" /p:PublishSingleFile=true

# Build Installer
dotnet publish "$installerProjectPath" -f net472 -o "$installerPublishDirectory"

# Build Installer (Static/Linux)
dotnet publish "$installerCliProjectPath" -f net8.0-windows -r win-x64 -o "$installerStaticPublishDirectory"

# Build Templates
dotnet pack "$templateProjectPath" -o "$templatePublishDirectory"

# Copy 32-bit EXE and cleanup folders.
Move-Item -Path "$outputPath32/Reloaded-II.exe" -Destination "$outputPath/Reloaded-II32.exe"
Remove-Item "$outputPath32" -Recurse -ErrorAction SilentlyContinue
Remove-Item "$loaderOutputPath/win-x86" -Recurse -ErrorAction SilentlyContinue
Remove-Item "$outputPath/win-x86" -Recurse -ErrorAction SilentlyContinue
Remove-Item "$outputPath/win-x64" -Recurse -ErrorAction SilentlyContinue
Remove-Item "$outputPath/ref" -Recurse -ErrorAction SilentlyContinue
Remove-Item "$outputPath/Theme/Halogen/Images/IconTemplate.7z" -Recurse -ErrorAction SilentlyContinue
Remove-Item "$outputPath/Theme/Default/R-II/Images/Templates.zip" -Recurse -ErrorAction SilentlyContinue

# Remove non-windows native stuff
$excludePatterns=@('win-x86*','win-x64*','win*')
$includePatterns=@('win-arm*')
Get-ChildItem "$outputPath/runtimes" -Exclude $excludePatterns | Remove-Item -Force -Recurse
Get-ChildItem "$outputPath/runtimes" -Include $includePatterns -Recurse | Remove-Item -Force -Recurse

# Remove debug/compile leftovers.
Get-ChildItem "$loader32OutputPath" -Include *.exe -Recurse -ErrorAction SilentlyContinue | Remove-Item -Force -Recurse
Get-ChildItem "$loader64OutputPath" -Include *.exe -Recurse -ErrorAction SilentlyContinue | Remove-Item -Force -Recurse
foreach ($cleanupPath in $cleanupPaths) {
    Get-ChildItem "$cleanupPath" -Include *.config -Recurse -ErrorAction SilentlyContinue | Remove-Item -Force -Recurse
    Get-ChildItem "$cleanupPath" -Include *.pdb -Recurse -ErrorAction SilentlyContinue | Remove-Item -Force -Recurse
    Get-ChildItem "$cleanupPath" -Include *.xml -Recurse -ErrorAction SilentlyContinue | Remove-Item -Force -Recurse
    Get-ChildItem "$cleanupPath" -Include *.exp -Recurse -ErrorAction SilentlyContinue | Remove-Item -Force -Recurse
    Get-ChildItem "$cleanupPath" -Include *.lib -Recurse -ErrorAction SilentlyContinue | Remove-Item -Force -Recurse
    Get-ChildItem "$cleanupPath" -Include *.iobj -Recurse -ErrorAction SilentlyContinue | Remove-Item -Force -Recurse
    Get-ChildItem "$cleanupPath" -Include *.ipdb -Recurse -ErrorAction SilentlyContinue | Remove-Item -Force -Recurse
}

# Make directories for storing results.
New-Item "$chocoPublishDirectory" -ItemType Directory -ErrorAction SilentlyContinue

# Compress result.
Add-Type -A System.IO.Compression.FileSystem
[IO.Compression.ZipFile]::CreateFromDirectory("$toolsPath", "$publishDirectory" + "$toolsReleaseFileName")

# Publish Mod
Get-ChildItem -Path "$outputPath" -Recurse -Force -ErrorAction SilentlyContinue
New-Item "$publishDirectory/$releaseFolder" -ItemType Directory -ErrorAction SilentlyContinue
./Publish.Reloaded.Release.ps1 -Version "$Version" -CurrentVersionPath "$outputPath" -ReleasePath "$publishDirectory/$releaseFolder"

Remove-Item "$chocoToolsPath" -Recurse -ErrorAction SilentlyContinue
New-Item "$chocoToolsPath" -ItemType Directory -ErrorAction SilentlyContinue
Copy-Item "$toolsPath/*" -Destination "$chocoToolsPath"
choco pack ./Chocolatey/reloaded-ii-tools.nuspec --out "$chocoPublishDirectory"

# Cleanup build items
Remove-Item "$buildPath" -Recurse

# Show Publish Items
Write-Host "Publish Items"
Get-ChildItem -Path $publishDirectory -Recurse -Force -ErrorAction SilentlyContinue

# Restore Working Directory
Pop-Location