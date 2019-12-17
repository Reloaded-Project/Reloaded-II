$outputPath = "Output/Launcher"
$launcherProjectPath = "Reloaded.Mod.Launcher/Reloaded.Mod.Launcher.csproj"
$publishDirectory = "Publish"
$releaseFileName = "/Release.zip"

# Clean output directory
Get-ChildItem $outputPath -Include * -Recurse | Remove-Item -Force -Recurse
Get-ChildItem $publishDirectory -Include * -Recurse | Remove-Item -Force -Recurse

# Build using Visual Studio & Dotnet Publish
devenv Reloaded-II.sln /Rebuild Release
dotnet publish $launcherProjectPath -c Release -r win-x64 --self-contained false -o $outputPath /p:PublishReadyToRun=true
Remove-Item "$outputPath/win-x64" -Recurse

# Remove debug/compile leftovers.
Get-ChildItem $outputPath -Include *.pdb -Recurse | Remove-Item -Force -Recurse
Get-ChildItem $outputPath -Include *.xml -Recurse | Remove-Item -Force -Recurse
Get-ChildItem $outputPath -Include *.exp -Recurse | Remove-Item -Force -Recurse
Get-ChildItem $outputPath -Include *.lib -Recurse | Remove-Item -Force -Recurse
Get-ChildItem $outputPath -Include *.iobj -Recurse | Remove-Item -Force -Recurse
Get-ChildItem $outputPath -Include *.ipdb -Recurse | Remove-Item -Force -Recurse

# Make compressed directory
if (! [System.IO.Directory]::Exists($publishDirectory)) {
    New-Item $publishDirectory -ItemType Directory
}

# Compress result.
Add-Type -A System.IO.Compression.FileSystem
[IO.Compression.ZipFile]::CreateFromDirectory($outputPath, $publishDirectory + $releaseFileName)