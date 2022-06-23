<#
.SYNOPSIS
    Creates Packages for a Release of Reloaded-II#

.PARAMETER Version
    Current version of Reloaded-II.

.PARAMETER CurrentVersionPath
    Path to current version on Disk.

.PARAMETER ReleasePath
    Where to save the release for upload.

#>
[cmdletbinding()]
param (
    ## => User Config <= ## 
    $Version = "1.0.0",
    $CurrentVersionPath = "./CurrentVersion",
    $ReleasePath = "./Publish/Release"
)

$PackagesPath = "./Publish/Packages"
$IgnoreRegexesPath = "./Publish-Settings/Ignore-Regexes.txt"
$IncludeRegexesPath = "./Publish-Settings/Include-Regexes.txt"
$PackagesListPath = "./Publish-Settings/Packages.txt"
Remove-Item "$PackagesPath" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item "$ReleasePath" -Recurse -Force -ErrorAction SilentlyContinue

## Download Update Tool
if (-Not (Test-Path -Path 'Sewer56.Update.Tool')) {
    Invoke-WebRequest -Uri "https://github.com/Sewer56/Update/releases/latest/download/Sewer56.Update.Tool.zip" -OutFile "Sewer56.Update.Tool.zip"

    ## Extract Tools
    Expand-Archive -LiteralPath './Sewer56.Update.Tool.zip' -DestinationPath "Sewer56.Update.Tool"
    Remove-Item './Sewer56.Update.Tool.zip' -Recurse -Force
}

## Generate Package 
$toolPath = "./Sewer56.Update.Tool/Sewer56.Update.Tool.dll"
dotnet $toolPath CreateCopyPackage --folderpath "$CurrentVersionPath" --version "$Version" --outputpath "$PackagesPath/current-version-package" --ignoreregexespath "$IgnoreRegexesPath" --includeregexespath "$IncludeRegexesPath"

## Uncomment for 2nd update and above
$LastVersion = & dotnet $toolPath DownloadPackage --outputpath "$PackagesPath/last-version-package" --source "GitHub" --githubusername "Reloaded-Project" --githubrepositoryname "Reloaded-II" --githublegacyfallbackpattern "Release.zip" --extract

dotnet $toolPath CreateDeltaPackage --lastversionfolderpath "$PackagesPath/last-version-package" --lastversion "$LastVersion" --folderpath "$PackagesPath/current-version-package" --version "$Version" --outputpath "$PackagesPath/delta-package-path"  --ignoreregexespath "$IgnoreRegexesPath" --includeregexespath "$IncludeRegexesPath"

## Create Release
dotnet $toolPath CreateRelease --existingpackagespath "$PackagesListPath" --outputpath "$ReleasePath" --packagename "Release" --dontappendversiontopackages
Remove-Item "$PackagesPath" -Recurse -Force