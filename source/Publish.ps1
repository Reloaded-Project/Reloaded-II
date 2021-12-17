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
$launcherProjectPath = "Reloaded.Mod.Launcher/Reloaded.Mod.Launcher.csproj"
$loaderProjectPath = "Reloaded.Mod.Loader/Reloaded.Mod.Loader.csproj"
$addressDumperProjectPath = "Reloaded.Mod.Launcher.Kernel32AddressDumper/Reloaded.Mod.Launcher.Kernel32AddressDumper.csproj"
$nugetConverterProjectPath = "Tools/NugetConverter/NugetConverter.csproj"

# Outputs
$publishDirectory = "Publish"
$chocoPublishDirectory = "$publishDirectory/Chocolatey"
$installerPublishDirectory = "$publishDirectory/Installer"
$releaseFileName = "/Release.zip"
$toolsReleaseFileName = "/Tools.zip"
$cleanupPaths = ("$buildPath", "$toolsPath", "$publishDirectory", "$chocoToolsPath")

# Set Working Directory
Split-Path $MyInvocation.MyCommand.Path | Push-Location
[Environment]::CurrentDirectory = $PWD
Write-Host "New Current Directory: $(Get-Location)"

# Clean output directories
foreach ($cleanupPath in $cleanupPaths) {
    Get-ChildItem "$cleanupPath" -Include * -Recurse -ErrorAction SilentlyContinue | Remove-Item -Force -Recurse -ErrorAction SilentlyContinue
}

Get-ChildItem "$chocoPath" -Include *.nupkg -Recurse -ErrorAction SilentlyContinue | Remove-Item -Force -Recurse

# Build using Visual Studio
msbuild $bootstrapperPath /p:Configuration=Release /p:Platform=x64 /p:OutDir="$loaderOutputPath"
dotnet publish "$addressDumperProjectPath" -c Release -r win-x86 --self-contained false -o "$loaderOutputPath"

# Build AnyCPU, and then copy 32-bit AppHost. 
dotnet publish "$launcherProjectPath" -c Release --self-contained false -o "$outputPath"
dotnet publish "$launcherProjectPath" -c Release -r win-x86 --self-contained false -o "$outputPath32"

# Build Loader
dotnet publish "$loaderProjectPath" -c Release -r win-x86 --self-contained false -o "$loaderOutputPath"

# Build Tools
dotnet publish "$nugetConverterProjectPath" -c Release -r win-x64 --self-contained false -o "$toolsPath" /p:PublishSingleFile=true

# Build Installer
dotnet publish "$installerProjectPath" -o "$installerPublishDirectory"

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
[IO.Compression.ZipFile]::CreateFromDirectory("$outputPath", "$publishDirectory" + "$releaseFileName")
[IO.Compression.ZipFile]::CreateFromDirectory("$toolsPath", "$publishDirectory" + "$toolsReleaseFileName")

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


