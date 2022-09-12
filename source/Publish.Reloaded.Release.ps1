<#
.SYNOPSIS
    Creates Packages for a Release of Reloaded-II#

.PARAMETER Version
    Current version of Reloaded-II.

.PARAMETER CurrentVersionPath
    Path to current version on Disk.

.PARAMETER ReleasePath
    Where to save the release for upload.

.PARAMETER NumberOfDeltaReleases
    The number of delta releases to create.

#>
[cmdletbinding()]
param (
    ## => User Config <= ## 
    $Version = "1.0.0",
    $CurrentVersionPath = "./CurrentVersion",
    $ReleasePath = "./Publish/Release",
    $NumberOfDeltaReleases = 3
)

$PackagesPath = "./Publish/Packages"
$IgnoreRegexesPath = "./Publish-Settings/Ignore-Regexes.txt"
$IncludeRegexesPath = "./Publish-Settings/Include-Regexes.txt"
$PackagesListBasePath = "./Publish-Settings/Packages.txt"
$PackagesListPath = "$PackagesPath/Packages.txt"

Remove-Item "$PackagesPath" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item "$ReleasePath" -Recurse -Force -ErrorAction SilentlyContinue
New-Item "$PackagesPath" -ItemType Directory -ErrorAction SilentlyContinue
Copy-Item -Path "$PackagesListBasePath" -Destination "$PackagesListPath"

## Download Update Tool
if (-Not (Test-Path -Path 'Sewer56.Update.Tool')) {
    Invoke-WebRequest -Uri "https://github.com/Sewer56/Update/releases/latest/download/Sewer56.Update.Tool.zip" -OutFile "Sewer56.Update.Tool.zip"

    ## Extract Tools
    Expand-Archive -LiteralPath './Sewer56.Update.Tool.zip' -DestinationPath "Sewer56.Update.Tool"
    Remove-Item './Sewer56.Update.Tool.zip' -Recurse -Force
}

## Generate Package 
Write-Host "Creating Current Version Package"
$toolPath = "./Sewer56.Update.Tool/Sewer56.Update.Tool.dll"
dotnet $toolPath CreateCopyPackage --folderpath "$CurrentVersionPath" --version "$Version" --outputpath "$PackagesPath/current-version-package" --ignoreregexespath "$IgnoreRegexesPath" --includeregexespath "$IncludeRegexesPath"

## Uncomment for 2nd update and above
Write-Host "Creating Deltas"
dotnet $toolPath AutoCreateDelta --outputpath "$PackagesPath" --source GitHub --folderpath "$CurrentVersionPath" --version "$Version" --githubusername "Reloaded-Project" --githubrepositoryname "Reloaded-II" --githublegacyfallbackpattern "Release.zip" --numreleases $NumberOfDeltaReleases --ignoreregexespath "$IgnoreRegexesPath" --includeregexespath "$IncludeRegexesPath" --noprogressbar | Out-File -FilePath "$PackagesListPath" -Encoding utf8 -Append 

## Create Release
Write-Host "Creating Release"
dotnet $toolPath CreateRelease --existingpackagespath "$PackagesListPath" --outputpath "$ReleasePath" --packagename "Release" --dontappendversiontopackages
Remove-Item "$PackagesPath" -Recurse -Force