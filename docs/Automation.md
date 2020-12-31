# Automation

This page details various different tasks you can automate to make mod development easier.

## Appveyor

You can use AppVeyor to automate building every version of your mods, and more. For those familiar, or even a bit familiar, here is a template for `appveyor.yml` that builds all versions and automatically publishes tags to GitHub Releases.

```yaml
version: CI-{build}
image: Visual Studio 2019
init:
- ps: |-
    & choco upgrade chocolatey -y
    & choco install reloaded-ii-tools --version=1.0.0 -y
    if ($env:APPVEYOR_REPO_TAG -eq "true")
    {
        Update-AppveyorBuild -Version "$env:APPVEYOR_REPO_TAG_NAME"
    }
build_script:
- ps: |- 
    & ./Publish.ps1
    
    # Create NuGet Packages
    $publishDirectory = "./Publish"
    $allZips = Get-ChildItem $publishDirectory -Filter *.zip
    foreach ($publishFile in $allZips) 
    {
        $nupkgName = [System.IO.Path]::ChangeExtension($publishFile.FullName, ".nupkg")
        $fullZipPath = $publishFile.FullName
        NuGetConverter.exe "$fullZipPath" "$nupkgName"
    }
artifacts:
- path: ./Publish/*.zip
  name: Compiled Mod(s)
- path: ./Publish/*.nupkg
  name: Compiled NuGet Packages
deploy:
- provider: GitHub
  auth_token:
    secure: 8Lqo9jP/L0PP7rNCr/FOdV8fc13U3U4kmDY5n9RMajb70SnIjujZz9J4tSGb9rAk
  force_update: false
  on:
    APPVEYOR_REPO_TAG: true
```

Replace `secure: 8Lqo9jP/L0PP7rNCr/FOdV8fc13U3U4kmDY5n9RMajb70SnIjujZz9J4tSGb9rAk` with your own GitHub token. Refer to [Appveyor Deployment: GitHub](https://www.appveyor.com/docs/deployment/github/) for more info.

Please remember to **encrypt** your token! You should be using [this tool](https://ci.appveyor.com/tools/encrypt) to do so.

### Upload to Official Package Repository on Build

Add to your `deploy:` section.

```yaml
- provider: NuGet
  server: http://167.71.128.50:5000/
  api_key:
    secure: /Ayzh3D/4Otzg80B1jc/6ltVaugqU8TP4fn/b4KA0as=
  skip_symbols: true
  on:
    APPVEYOR_REPO_TAG: true
```

## Auto-Building Packages

### 1. Install NuGetPackageConverter

There also exists a standalone NuGet Package Converter; you can get it from [Chocolatey](https://chocolatey.org/install) (recommended) or from Github Releases.

If you are using AppVeyor, Chocolatey is already preinstalled. If you are using Github Actions, consider trying [Chocolatey Action](https://github.com/marketplace/actions/chocolatey-action).

Once chocolatey is installed, simply install the [Reloaded Tools](chocolatey.org/packages/reloaded-ii-tools): 
```
choco install reloaded-ii-tools -y
```

### 2. Build and Pack The Mod 

Build the mod using the [included script from the mod template](./DeveloperModGuide.md#4-publishing); output goes in the `Publish` folder.

Then simply pack all of the output using one of the tools installed earlier.

```powershell
# Build the Mod
./Publish.ps1

# Package all zips in Publish Folder
$publishDirectory = "./Publish"
$allZips = Get-ChildItem $publishDirectory -Filter *.zip

foreach ($publishFile in $allZips) 
{
	$nupkgName = [System.IO.Path]::ChangeExtension($publishFile.FullName, ".nupkg")
	$fullZipPath = $publishFile.FullName
	NuGetConverter.exe "$fullZipPath" "$nupkgName"
}
```

You can now add the package to your artifacts/output/etc.