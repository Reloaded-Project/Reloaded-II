# Single File Publish Currently Disabled because it's Broken with WPF.
# Only after the following bugs are fixed:
# - https://github.com/dotnet/wpf/issues/3516 was fixed but now there's a new problem
$publishSingleFile = $false

# Build Locations
$buildPath = "Publish-Build"
$outputPath = "$buildPath/Publish"
$outputPath32 = "$buildPath/Publish/x86"
$toolsPath  = "$buildPath/Tools/"
$loaderOutputPath = "$outputPath/Loader/"
$loader32OutputPath = "$outputPath/Loader/x86"
$loader64OutputPath = "$outputPath/Loader/x64"

# Project Paths
$bootstrapperPath = "Reloaded.Mod.Loader.Bootstrapper/Reloaded.Mod.Bootstrapper.vcxproj"
$launcherProjectPath = "Reloaded.Mod.Launcher/Reloaded.Mod.Launcher.csproj"
$loaderProjectPath = "Reloaded.Mod.Loader/Reloaded.Mod.Loader.csproj"
$addressDumperProjectPath = "Reloaded.Mod.Launcher.Kernel32AddressDumper/Reloaded.Mod.Launcher.Kernel32AddressDumper.csproj"
$nugetConverterProjectPath = "Tools/NugetConverter/NugetConverter.csproj"

# Outputs
$publishDirectory = "Publish"
$releaseFileName = "/Release.zip"
$toolsReleaseFileName = "/Tools.zip"
$cleanupPaths = ("$buildPath", "$toolsPath", "$publishDirectory")

# Set Working Directory
Split-Path $MyInvocation.MyCommand.Path | Push-Location
[Environment]::CurrentDirectory = $PWD

# Clean output directory
foreach ($cleanupPath in $cleanupPaths) {
    Get-ChildItem "$cleanupPath" -Include * -Recurse -ErrorAction SilentlyContinue | Remove-Item -Force -Recurse -ErrorAction SilentlyContinue
}

# Build using Visual Studio & Dotnet Publish
msbuild $bootstrapperPath /p:Configuration=Release /p:Platform=x64 /p:OutDir="../$loaderOutputPath"
dotnet build-server shutdown
dotnet publish "$addressDumperProjectPath" -c Release -r win-x86 --self-contained false /p:PublishSingleFile=true -o "$loaderOutputPath"

# Build AnyCPU, and then copy 32-bit AppHost. 
dotnet publish "$launcherProjectPath" -c Release --self-contained false -o "$outputPath"
dotnet publish "$launcherProjectPath" -c Release -r win-x86 --self-contained false -o "$outputPath32"

# Build Loader
dotnet publish "$loaderProjectPath" -c Release -r win-x86 --self-contained false -o "$loaderOutputPath"

# Build Tools
dotnet build-server shutdown
dotnet publish "$nugetConverterProjectPath" -c Release -r win-x64 --self-contained false -o "$toolsPath" /p:PublishSingleFile=true

# Copy 32-bit EXE and cleanup folders.
Move-Item -Path "$outputPath32/Reloaded-II.exe" -Destination "$outputPath/Reloaded-II32.exe"
Remove-Item "$outputPath32" -Recurse -ErrorAction SilentlyContinue
Remove-Item "$loaderOutputPath/win-x86" -Recurse -ErrorAction SilentlyContinue
Remove-Item "$outputPath/win-x86" -Recurse -ErrorAction SilentlyContinue
Remove-Item "$outputPath/win-x64" -Recurse -ErrorAction SilentlyContinue
Remove-Item "$outputPath/ref" -Recurse -ErrorAction SilentlyContinue

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

# Make compressed directory
Remove-Item "$publishDirectory" -Recurse
New-Item "$publishDirectory" -ItemType Directory

# Compress result.
Add-Type -A System.IO.Compression.FileSystem
[IO.Compression.ZipFile]::CreateFromDirectory("$outputPath", "$publishDirectory" + "$releaseFileName")
[IO.Compression.ZipFile]::CreateFromDirectory("$toolsPath", "$publishDirectory" + "$toolsReleaseFileName")

# Restore Working Directory
Pop-Location