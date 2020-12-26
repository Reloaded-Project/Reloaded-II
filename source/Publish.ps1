# Single File Publish Currently Disabled because it's Broken with WPF.
# Only after the following bugs are fixed:
# - https://github.com/dotnet/wpf/issues/3516 was fixed but now there's a new problem
$publishSingleFile = $false

# Build Locations
$buildPath = "Output"
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

[Environment]::CurrentDirectory = $PWD

# Clean output directory
foreach ($cleanupPath in $cleanupPaths) {
    Get-ChildItem "$cleanupPath" -Include * -Recurse | Remove-Item -Force -Recurse
}

# Build using Visual Studio & Dotnet Publish
dotnet restore	

msbuild $bootstrapperPath /p:Configuration=Release /p:Platform=x64 /p:OutDir="../$loaderOutputPath"
dotnet publish "$addressDumperProjectPath" -c Release -r win-x86 --self-contained false /p:PublishSingleFile=true -o "$loaderOutputPath"

if ($publishSingleFile) 
{
	dotnet publish "$launcherProjectPath" -c Release -r win-x64 --self-contained false /p:PublishReadyToRun=false /p:PublishSingleFile=true /p:Configuration=SingleFile -o "$outputPath"
	dotnet publish "$launcherProjectPath" -c Release -r win-x86 --self-contained false /p:PublishReadyToRun=false /p:PublishSingleFile=true /p:Configuration=SingleFile -o "$outputPath32"
}
else 
{
	dotnet publish "$launcherProjectPath" -c Release -r win-x64 --self-contained false /p:PublishReadyToRun=false -o "$outputPath"
	dotnet publish "$launcherProjectPath" -c Release -r win-x86 --self-contained false /p:PublishReadyToRun=false -o "$outputPath32"
}

dotnet publish "$loaderProjectPath" -c Release -r win-x64 --self-contained false -o "$loader64OutputPath" /p:PublishReadyToRun=true
dotnet publish "$loaderProjectPath" -c Release -r win-x86 --self-contained false -o "$loader32OutputPath" /p:PublishReadyToRun=true
dotnet publish "$nugetConverterProjectPath" -c Release -r win-x64 --self-contained false -o "$toolsPath" /p:PublishSingleFile=true

Move-Item -Path "$outputPath32/Reloaded-II.exe" -Destination "$outputPath/Reloaded-II32.exe"
Remove-Item "$outputPath32" -Recurse
Remove-Item "$loaderOutputPath/win-x86" -Recurse
Remove-Item "$outputPath/win-x86" -Recurse
Remove-Item "$outputPath/win-x64" -Recurse
Remove-Item "$outputPath/ref" -Recurse

# Remove debug/compile leftovers.
Get-ChildItem "$loader32OutputPath" -Include *.exe -Recurse | Remove-Item -Force -Recurse
Get-ChildItem "$loader64OutputPath" -Include *.exe -Recurse | Remove-Item -Force -Recurse
foreach ($cleanupPath in $cleanupPaths) {
    Get-ChildItem "$cleanupPath" -Include *.config -Recurse | Remove-Item -Force -Recurse
    Get-ChildItem "$cleanupPath" -Include *.pdb -Recurse | Remove-Item -Force -Recurse
    Get-ChildItem "$cleanupPath" -Include *.xml -Recurse | Remove-Item -Force -Recurse
    Get-ChildItem "$cleanupPath" -Include *.exp -Recurse | Remove-Item -Force -Recurse
    Get-ChildItem "$cleanupPath" -Include *.lib -Recurse | Remove-Item -Force -Recurse
    Get-ChildItem "$cleanupPath" -Include *.iobj -Recurse | Remove-Item -Force -Recurse
    Get-ChildItem "$cleanupPath" -Include *.ipdb -Recurse | Remove-Item -Force -Recurse
}

# Make compressed directory
Remove-Item "$publishDirectory" -Recurse
New-Item "$publishDirectory" -ItemType Directory

# Compress result.
Add-Type -A System.IO.Compression.FileSystem
[IO.Compression.ZipFile]::CreateFromDirectory("$outputPath", "$publishDirectory" + "$releaseFileName")
[IO.Compression.ZipFile]::CreateFromDirectory("$toolsPath", "$publishDirectory" + "$toolsReleaseFileName")