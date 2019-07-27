$outputPath = "Output/Launcher"
$publishDirectory = "Publish"
$releaseFileName = "/Release.zip"

# Clean output directory
Get-ChildItem $outputPath -Include * -Recurse | Remove-Item -Force -Recurse
Get-ChildItem $publishDirectory -Include * -Recurse | Remove-Item -Force -Recurse

# Build using Visual Studio
devenv Reloaded-II.sln /Rebuild Release

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