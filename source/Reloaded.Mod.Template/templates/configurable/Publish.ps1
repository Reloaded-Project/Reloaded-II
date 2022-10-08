<#
.SYNOPSIS
    Builds and Publishes a Reloaded II Mod
.DESCRIPTION
    Windows script to Build and Publish a Reloaded Mod.
    By default, published items will be output to a directory called `Publish/ToUpload`.

    If you acquired this script by creating a new Reloaded Mod in VS. Then most likely everything 
    (aside from delta updates) should be preconfigured here.

.PARAMETER ProjectPath
    Path to the project to be built.
    Useful if using this script from another script for the purpose of building multiple mods.

.PARAMETER PackageName
    Name of the package to be built.
    Affects the name of the output files of the publish.

.PARAMETER PublishOutputDir
    Default: "Publish/ToUpload"
    Declares the directory for placing the output files.

.PARAMETER BuildR2R
    Default: $False

    Builds the mod using an optimisation called `Ready to Run`, which sacrifices file size for potentially
    faster startup time. This is only worth enabling on mods with a lot of code, usually it is best left disabled. 

    For more details see: https://docs.microsoft.com/en-us/dotnet/core/deploying/ready-to-run

.PARAMETER ChangelogPath
    Full or relative path to a file containing the changelog for the mod.
    The changelog should be written in Markdown format.

.PARAMETER ReadmePath
    Full or relative path to a file containing the changelog for the mod.
    The changelog should be written in Markdown format.

.PARAMETER IsPrerelease
    Default: $False

    If set to true, the version downloaded for delta package generation will be the latest pre-release
    as opposed to the latest stable version.

.PARAMETER MakeDelta
    Default: $False

    Set to true to create Delta packages.
    Usually this is true in a CI/CD environment when creating a release, else false in development.

    If this is true, you should set UseGitHubDelta, UseGameBananaDelta, UseNuGetDelta or equivalent to true.

.PARAMETER MetadataFileName
    Default: Sewer56.Update.ReleaseMetadata.json
    Name of the release metadata file used to download the delta package.

.PARAMETER UseGitHubDelta
    Default: $False
    If true, sources the last version of the package to publish from GitHub.

.PARAMETER UseGameBananaDelta
    Default: $False
    If true, sources the last version of the package to publish from GameBanana.

.PARAMETER UseNuGetDelta
    Default: $False
    If true, sources the last version of the package to publish from NuGet.

.PARAMETER GitHubUserName
    [Use if UseGitHubDelta is true]
    Sets the username used for obtaining Deltas from GitHub.

.PARAMETER GitHubRepoName
    [Use if UseGitHubDelta is true]
    Sets the repository used for obtaining Deltas from GitHub.

.PARAMETER GitHubFallbackPattern
    [Use if UseGitHubDelta is true]
    Allows you to specify a Wildcard pattern (e.g. *Update.zip) for the file to be downloaded.
    This is a fallback used in cases no Release Metadata file can be found.

.PARAMETER GitHubInheritVersionFromTag
    [Use if UseGitHubDelta is true]
    Uses version determined from release tag (in GitHub Releases) as opposed to the 
    Release Metadata file in latest release.

.PARAMETER GameBananaItemId
    [Use if UseGameBananaDelta is true]
    Example: 150118

    Unique identifier for the individual mod. This is the last number of a GameBanana Mod Page URL
    e.g. https://gamebanana.com/mods/150118 -> 150118

.PARAMETER NuGetPackageId
    [Use if UseNuGetDelta is true]
    Example: reloaded.sharedlib.hooks

    The ID of the package to use as delta.

.PARAMETER NuGetFeedUrl
    [Use if UseNuGetDelta is true]
    Example: http://packages.sewer56.moe:5000/v3/index.json

    The URL of the NuGet feed to download the delta from.

.PARAMETER NuGetAllowUnlisted
    [Use if UseNuGetDelta is true]
    Default: $False

    Allows for the downloading of unlisted packages.

.PARAMETER PublishGeneric
    Default: $True

    Publishes a generic package that can be uploaded to any other website.
    
.PARAMETER PublishNuGet
    Default: $True

    Publishes a package that can be uploaded to any NuGet Source.

.PARAMETER PublishGameBanana
    Default: $True

    Publishes a package that can be uploaded to GameBanana.

.PARAMETER Build
    Default: $True

    Whether the project should be built.
    Setting this to false lets you use the publish part of the script standalone in a non .NET environment.
	
.PARAMETER RemoveExe
    Default: $True

    Removes executables from build output. Useful when performing R2R Optimisation.

.EXAMPLE
  .\Publish.ps1 -ProjectPath "Reloaded.Hooks.ReloadedII/Reloaded.Hooks.ReloadedII.csproj" -PackageName "Reloaded.Hooks.ReloadedII" -PublishOutputDir "Publish/ToUpload"

.EXAMPLE
  .\Publish.ps1 -MakeDelta true -BuildR2R true -UseGitHubDelta True

.EXAMPLE
  .\Publish.ps1 -BuildR2R true

#>
[cmdletbinding()]
param (
    $IsPrerelease=$False, 
    $MakeDelta=$False, 
    $ChangelogPath="",
    $ReadmePath="",
    $Build=$True,
    $BuildR2R=$False,
    $RemoveExe = $True,
	
    ## => User Config <= ## 
    $ProjectPath = "Reloaded.Mod.Template.csproj",
    $PackageName = "Reloaded.Mod.Template",
    $PublishOutputDir = "Publish/ToUpload",

    ## => User: Delta Config
    # Pick one and configure settings below.
    $MetadataFileName = "Sewer56.Update.ReleaseMetadata.json",
    $UseGitHubDelta = $False, # GitHub Releases
    $UseGameBananaDelta = $False,
    $UseNuGetDelta = $False,

    $GitHubUserName = "", # Name of the GitHub user where the mod is contained
    $GitHubRepoName = "", # Name of the GitHub repo where the mod is contained
    $GitHubFallbackPattern = "", # For migrating from legacy build script.
    $GitHubInheritVersionFromTag = $True, # Uses version determined from release tag as opposed to metadata file in latest release.

    $GameBananaItemId = 333681, # From mod page URL.

    $NuGetPackageId = "Reloaded.Mod.Template",
    $NuGetFeedUrl = "http://packages.sewer56.moe:5000/v3/index.json",
    $NuGetAllowUnlisted = $False,

    ## => User: Publish Config
    $PublishGeneric    = $True,
    $PublishNuGet      = $True,
    $PublishGameBanana = $True
)

## => User: Publish Output
$publishBuildDirectory = "Publish/Builds/CurrentVersion"      # Build directory for current version of the mod.
$deltaDirectory = "Publish/Builds/LastVersion"                # Path to last version of the mod.

$PublishGenericDirectory = "$PublishOutputDir/Generic"        # Publish files for any target not listed below.
$PublishNuGetDirectory   = "$PublishOutputDir/NuGet"          # Publish files for NuGet
$PublishGameBananaDirectory = "$PublishOutputDir/GameBanana"  # Publish files for GameBanana

## => User Config <= ## 
# Tools
$reloadedToolsPath = "./Publish/Tools/Reloaded-Tools"    # Used to check if tools are installed.
$updateToolsPath   = "./Publish/Tools/Update-Tools"      # Used to check if update tools are installed.
$reloadedToolPath = "$reloadedToolsPath/Reloaded.Publisher.exe"  # Path to Reloaded publishing tool.
$updateToolPath   = "$updateToolsPath/Sewer56.Update.Tool.dll" # Path to Update tool.
$changelogFullPath = $null
$readmeFullPath = $null
if ($ChangelogPath) { $changelogFullPath = [System.IO.Path]::GetFullPath($ChangelogPath) }
if ($ReadmePath) { $readmeFullPath = [System.IO.Path]::GetFullPath($ReadmePath) }

## => Script <= ##
# Set Working Directory
Split-Path $MyInvocation.MyCommand.Path | Push-Location
[Environment]::CurrentDirectory = $PWD

# Convert Booleans
$IsPrerelease = [bool]::Parse($IsPrerelease)
$MakeDelta = [bool]::Parse($MakeDelta)
$Build = [bool]::Parse($Build)
$BuildR2R = [bool]::Parse($BuildR2R)
$RemoveExe = [bool]::Parse($RemoveExe)
$UseGitHubDelta = [bool]::Parse($UseGitHubDelta)
$UseGameBananaDelta = [bool]::Parse($UseGameBananaDelta)
$UseNuGetDelta = [bool]::Parse($UseNuGetDelta)
$NuGetAllowUnlisted = [bool]::Parse($NuGetAllowUnlisted)
$PublishGeneric = [bool]::Parse($PublishGeneric)
$PublishNuGet = [bool]::Parse($PublishNuGet)
$PublishGameBanana = [bool]::Parse($PublishGameBanana)
$GitHubInheritVersionFromTag = [bool]::Parse($GitHubInheritVersionFromTag)
$TempDirectory = [System.IO.Path]::GetTempPath() + [System.IO.Path]::GetRandomFileName()
$TempDirectoryBuild = "$TempDirectory/build"

function Get-Tools {
    # Download Tools (if needed)
    $ProgressPreference = 'SilentlyContinue'
    if (-not(Test-Path -Path $reloadedToolsPath -PathType Any)) {
        Write-Host "Downloading Reloaded Tools"
        Invoke-WebRequest -Uri "https://github.com/Reloaded-Project/Reloaded-II/releases/latest/download/Tools.zip" -OutFile "$TempDirectory/Tools.zip"
        Expand-Archive -LiteralPath "$TempDirectory/Tools.zip" -DestinationPath $reloadedToolsPath

        # Remove Items
        Remove-Item "$TempDirectory/Tools.zip" -ErrorAction SilentlyContinue
    }

    if ($MakeDelta -and -not(Test-Path -Path $updateToolsPath -PathType Any)) {
        Write-Host "Downloading Update Library Tools"
        Invoke-WebRequest -Uri "https://github.com/Sewer56/Update/releases/latest/download/Sewer56.Update.Tool.zip" -OutFile "$TempDirectory/Sewer56.Update.Tool.zip"
        Expand-Archive -LiteralPath "$TempDirectory/Sewer56.Update.Tool.zip" -DestinationPath $updateToolsPath

        # Remove Items
        Remove-Item "$TempDirectory/Sewer56.Update.Tool.zip" -ErrorAction SilentlyContinue    
    }
}

# Publish for targets
function Build {
    # Clean anything in existing Release directory.
    Remove-Item $publishBuildDirectory -Recurse -ErrorAction SilentlyContinue
    New-Item $publishBuildDirectory -ItemType Directory -ErrorAction SilentlyContinue

    # Build
    dotnet restore $ProjectPath
    dotnet clean $ProjectPath

    if ($BuildR2R) {
        dotnet publish $ProjectPath -c Release -r win-x86 --self-contained false -o "$publishBuildDirectory/x86" /p:PublishReadyToRun=true /p:OutputPath="$TempDirectoryBuild/x86"
        dotnet publish $ProjectPath -c Release -r win-x64 --self-contained false -o "$publishBuildDirectory/x64" /p:PublishReadyToRun=true /p:OutputPath="$TempDirectoryBuild/x64"

        # Remove Redundant Files
        Move-Item -Path "$publishBuildDirectory/x86/ModConfig.json" -Destination "$publishBuildDirectory/ModConfig.json" -ErrorAction SilentlyContinue
        Move-Item -Path "$publishBuildDirectory/x86/Preview.png" -Destination "$publishBuildDirectory/Preview.png" -ErrorAction SilentlyContinue
        Remove-Item "$publishBuildDirectory/x64/Preview.png" -ErrorAction SilentlyContinue
        Remove-Item "$publishBuildDirectory/x64/ModConfig.json" -ErrorAction SilentlyContinue
    }
    else {
        dotnet publish $ProjectPath -c Release --self-contained false -o "$publishBuildDirectory" /p:OutputPath="$TempDirectoryBuild"
    }

    # Cleanup Unnecessary Files
    Remove-Item $TempDirectoryBuild -Recurse -ErrorAction SilentlyContinue
	if ($RemoveExe) {
        Get-ChildItem $publishBuildDirectory -Include *.exe -Recurse | Remove-Item -Force -Recurse
	}
	
    Get-ChildItem $publishBuildDirectory -Include *.pdb -Recurse | Remove-Item -Force -Recurse
    Get-ChildItem $publishBuildDirectory -Include *.xml -Recurse | Remove-Item -Force -Recurse
}

function Get-Last-Version {
    
    Remove-Item $deltaDirectory -Recurse -ErrorAction SilentlyContinue
    New-Item $deltaDirectory -ItemType Directory -ErrorAction SilentlyContinue
    $arguments = "DownloadPackage --extract --outputpath `"$deltaDirectory`" --allowprereleases `"$IsPrerelease`" --metadatafilename `"$MetadataFileName`""
	
    if ($UseGitHubDelta) {
        $arguments += " --source GitHub --githubusername `"$GitHubUserName`" --githubrepositoryname `"$GitHubRepoName`" --githublegacyfallbackpattern `"$GitHubFallbackPattern`" --githubinheritversionfromtag `"$GitHubInheritVersionFromTag`""
    }
    elseif ($UseNuGetDelta) {
        $arguments += " --source NuGet --nugetpackageid `"$NuGetPackageId`" --nugetfeedurl `"$NuGetFeedUrl`" --nugetallowunlisted `"$NuGetAllowUnlisted`""
    }
    elseif ($UseGameBananaDelta) {
        $arguments += " --source GameBanana --gamebananaitemid `"$GameBananaItemId`""
    }

	Invoke-Expression "dotnet `"$updateToolPath`" $arguments"
}

function Get-Common-Publish-Args {
	
	param (
        $AllowDeltas=$True
    )
	
	$arguments = "--modfolder `"$publishBuildDirectory`" --packagename `"$PackageName`""
	if ($ChangelogPath) {
        $arguments += " --changelogpath `"$changelogFullPath`""
	}

    if ($ReadmePath) {
        $arguments += " --readmepath `"$readmeFullPath`""
	}
	
	if ($AllowDeltas -and $MakeDelta) {
        $arguments += " --olderversionfolders `"$deltaDirectory`""
	}
	
	return $arguments
}

function Publish-Common {

	param (
        $Directory="",
        $AllowDeltas=$True,
        $PublishTarget=""
    )
    
    Remove-Item $Directory -Recurse -ErrorAction SilentlyContinue
    New-Item $Directory -ItemType Directory -ErrorAction SilentlyContinue
	$arguments = "$(Get-Common-Publish-Args -AllowDeltas $AllowDeltas) --outputfolder `"$Directory`" --publishtarget $PublishTarget"
	$command = "$reloadedToolPath $arguments"
	Write-Host "$command`r`n`r`n"
	Invoke-Expression $command
}

function Publish-GameBanana {
    Publish-Common -Directory $PublishGameBananaDirectory -PublishTarget GameBanana
}

function Publish-NuGet {
    Publish-Common -Directory $PublishNuGetDirectory -PublishTarget NuGet -AllowDeltas $False
}

function Publish-Generic {
    Publish-Common -Directory $PublishGenericDirectory -PublishTarget Default
}

function Cleanup {
    Remove-Item $PublishOutputDir -Recurse -ErrorAction SilentlyContinue
    Remove-Item $PublishNuGetDirectory -Recurse -ErrorAction SilentlyContinue
    Remove-Item $PublishGenericDirectory -Recurse -ErrorAction SilentlyContinue
    Remove-Item $publishBuildDirectory -Recurse -ErrorAction SilentlyContinue
    Remove-Item $deltaDirectory -Recurse -ErrorAction SilentlyContinue
}

# Build & Publish
New-Item $TempDirectory -ItemType Directory -ErrorAction SilentlyContinue
Cleanup
Get-Tools

if ($MakeDelta) {
    Write-Host "Downloading Delta (Last Version)"
    Get-Last-Version
}

if ($Build) {
    Write-Host "Building Mod"
    Build    
}

if ($PublishGeneric) {
    Write-Host "Publishing Mod for Default Target"
    Publish-Generic
}

if ($PublishNuGet) {
    Write-Host "Publishing Mod for NuGet Target"
    Publish-NuGet
}

if ($PublishGameBanana) {
    Write-Host "Publishing Mod for GameBanana Target"
    Publish-GameBanana
}

# Remove Temp Folder
Remove-Item $TempDirectory -Recurse -ErrorAction SilentlyContinue

# Restore Working Directory
Write-Host "Done."
Write-Host "Upload the files in folder `"$PublishOutputDir`" to respective location or website."
Pop-Location