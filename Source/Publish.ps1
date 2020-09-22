# Set to true to build .NET 5
# Only after the following bugs are fixed:
# - https://github.com/dotnet/wpf/issues/3516 (Affects Launcher)
# - https://github.com/dotnet/runtime/issues/39176 (Affects Loader)
# For now, please edit `TargetFramework` in Reloaded.Mod.Launcher, Reloaded.Mod.Loader and NuGetConverter manually before flipping this switch.
$isNet5 = $false

# Build Locations
$outputPath = "Output/Launcher/"
$outputPath32 = "Output/Launcher/x86"
$toolsPath  = "Output/Tools/"
$dumperOutputPath = "$outputPath/Loader/"
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
$cleanupPaths = ("$outputPath", "$toolsPath", "$publishDirectory")

[Environment]::CurrentDirectory = $PWD

# Clean output directory
foreach ($cleanupPath in $cleanupPaths) {
    Get-ChildItem "$cleanupPath" -Include * -Recurse | Remove-Item -Force -Recurse
}

# Build using Visual Studio & Dotnet Publish
dotnet restore	

devenv Reloaded-II.sln /Project Reloaded.Mod.Loader.Bootstrapper /Build Release
dotnet publish "$addressDumperProjectPath" -c Release -r win-x86 --self-contained false /p:PublishSingleFile=true -o "$dumperOutputPath"

if ($isNet5) 
{
    dotnet publish "$launcherProjectPath" -c Release -r win-x64 -f net5.0-windows --self-contained false /p:PublishReadyToRun=false /p:PublishSingleFile=true -o "$outputPath"
    dotnet publish "$launcherProjectPath" -c Release -r win-x86 -f net5.0-windows --self-contained false /p:PublishReadyToRun=false /p:PublishSingleFile=true -o "$outputPath32"
}
else 
{
    dotnet publish "$launcherProjectPath" -c Release -f netcoreapp3.1 --self-contained false -o "$outputPath"
}

dotnet publish "$loaderProjectPath" -c Release -r win-x64 --self-contained false -o "$loader64OutputPath" /p:PublishReadyToRun=true
dotnet publish "$loaderProjectPath" -c Release -r win-x86 --self-contained false -o "$loader32OutputPath" /p:PublishReadyToRun=true
dotnet publish "$nugetConverterProjectPath" -c Release -r win-x64 --self-contained false -o "$toolsPath" /p:PublishSingleFile=true

Move-Item -Path "$outputPath32/Reloaded-II.exe" -Destination "$outputPath/Reloaded-II32.exe"
Remove-Item "$outputPath32" -Recurse
Remove-Item "$dumperOutputPath/win-x86" -Recurse
Remove-Item "$outputPath/win-x86" -Recurse
Remove-Item "$outputPath/win-x64" -Recurse
Remove-Item "$outputPath/ref" -Recurse
Remove-Item "$outputPath/runtimes" -Recurse # Potentially Dangerous

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