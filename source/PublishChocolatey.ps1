# Reminder
Write-Host "Reminder: This script is to be executed after running Publish.ps1"

$buildPath = "Output"
$toolsPath  = "$buildPath/Tools/"
$chocoPath = "./Chocolatey"
$publishDirectory = "./Publish"
$chocoToolsPath = "$chocoPath/tools"

# Set Working Directory
Split-Path $MyInvocation.MyCommand.Path | Push-Location
[Environment]::CurrentDirectory = $PWD

# Clean Choco Tools Folder
Write-Host "Cleaning Chocolatey Tools Folder"

foreach ($cleanupPath in $chocoToolsPath) {
    Get-ChildItem "$cleanupPath" -Include *.exe -Recurse | Remove-Item -Force -Recurse
	Get-ChildItem "$cleanupPath" -Include *.dll -Recurse | Remove-Item -Force -Recurse
}

# Copying to Tools Folder
Write-Host "Copying Output Binaries"
Copy-Item "$toolsPath/*" -Destination "$chocoToolsPath"

# Create choco package.
Write-Host "Creating Choco Package"
Push-Location $chocoPath
choco pack
Pop-Location

# Restore Working Directory
Write-Host "Copying Package to $publishDirectory"
Copy-Item "$chocoPath/*.nupkg" -Destination "$publishDirectory"
Pop-Location