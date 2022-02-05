# Automation

This page details various different tasks you can automate to make mod development easier.

## CI/CD

Below is a sample template for doing CI/CD using GitHub Actions:  

```yaml
name: Build and Publish

on:
  push:
    branches: [ main ]
    tags:
      - '*'
  pull_request:
    branches: [ main ]
  workflow_dispatch:

env: 
  PUBLISH_GITHUB_PATH: ./Publish/ToUpload/Generic
  PUBLISH_NUGET_PATH: ./Publish/ToUpload/NuGet
  PUBLISH_CHANGELOG_PATH: ./Publish/Changelog.md
  PUBLISH_PATH: ./Publish
  
  IS_RELEASE: ${{ startsWith(github.ref, 'refs/tags/') }}
  RELEASE_TAG: ${{ github.ref_name }}

jobs:
  build:
    runs-on: windows-latest
    defaults:
      run:
        shell: pwsh
    
    steps:
    - uses: actions/checkout@v2
      with:
        fetch-depth: 0

    - name: Setup .NET Core SDK (5.0)
      uses: actions/setup-dotnet@v1.8.2
      with:
        dotnet-version: 5.0.x
        
    - name: Setup Node.js
      uses: actions/setup-node@v2
      with:
        node-version: '14'
        
    - name: Setup AutoChangelog
      run: npm install -g auto-changelog

    - name: Build
      run: ./Publish.ps1
      
    - name: Create Changelog
      run: |
        [System.IO.Directory]::CreateDirectory("$env:PUBLISH_PATH")
        if ($env:IS_RELEASE -eq 'true') {
            auto-changelog --sort-commits date --hide-credit --template keepachangelog --commit-limit false --unreleased --starting-version "$env:RELEASE_TAG" --output "$env:PUBLISH_CHANGELOG_PATH"
        }
        else {
            auto-changelog --sort-commits date --hide-credit --template keepachangelog --commit-limit false --unreleased --output "$env:PUBLISH_CHANGELOG_PATH"
        }
         
    - name: Upload GitHub Release Artifact
      uses: actions/upload-artifact@v2.2.4
      with:
        # Artifact name
        name: GitHub Release
        # A file, directory or wildcard pattern that describes what to upload
        path: ${{ env.PUBLISH_GITHUB_PATH }}/*
        
    - name: Upload NuGet Release Artifact
      uses: actions/upload-artifact@v2.2.4
      with:
        # Artifact name
        name: NuGet Release
        # A file, directory or wildcard pattern that describes what to upload
        path: ${{ env.PUBLISH_NUGET_PATH }}/*
        
    - name: Upload Changelog Artifact
      uses: actions/upload-artifact@v2.2.4
      with:
        # Artifact name
        name: Changelog
        # A file, directory or wildcard pattern that describes what to upload
        path: ${{ env.PUBLISH_CHANGELOG_PATH }}
        retention-days: 0
    
    - name: Upload to GitHub Releases
      uses: softprops/action-gh-release@v0.1.14
      if: env.IS_RELEASE == 'true'
      with:
        # Path to load note-worthy description of changes in release from
        body_path: ${{ env.PUBLISH_CHANGELOG_PATH }}
        # Newline-delimited list of path globs for asset files to upload
        files: |
          ${{ env.PUBLISH_GITHUB_PATH }}/*


```

Feel free to modify any parts of this template.  
Note: You may need to change branch name from `main` to `master` for older repos.  

### Pushing NuGet Packages

You can push packages to a NuGet repository using the following step:  

```yaml
- name: Push to Official NuGet Repository (on Tag)
  env: 
    NUGET_KEY: ${{ secrets.RELOADED_NUGET_KEY }}
  if: env.IS_RELEASE == 'true'
  run: |
    $items = Get-ChildItem -Path "$env:PUBLISH_NUGET_PATH/*.nupkg"
    Foreach ($item in $items)
    {
        Write-Host "Pushing $item"
        dotnet nuget push "$item" -k "$env:NUGET_KEY" -s "http://packages.sewer56.moe:5000/v3/index.json" --skip-duplicate
    }
```

Make sure to add your NuGet API key as a [https://docs.github.com/en/actions/security-guides/encrypted-secrets#creating-encrypted-secrets-for-a-repository](Secret). In this snippet the secret is named `RELOADED_NUGET_KEY`.

The source `http://packages.sewer56.moe:5000/v3/index.json` in this snippet points to the official Reloaded II NuGet repository.

### Creating Delta Updates

Please see [Publishing Mods: Publish Script](PublishingMods.md#creating-releases-publish-script).

Example(s):  
```powershell
# Publish using GitHub Releases as the delta source.
./Publish.ps1 -MakeDelta true -UseGitHubDelta true -GitHubUserName Sewer56 -GitHubRepoName Reloaded.SharedLib.Hooks.ReloadedII -GitHubFallbackPattern reloaded.sharedlib.hooks.zip
```

```powershell
# Publish using NuGet as the delta source.
./Publish.ps1 -MakeDelta true -UseNuGetDelta true -NuGetPackageId reloaded.sharedlib.hooks -NuGetFeedUrl http://packages.sewer56.moe:5000/v3/index.json
```

This goes in place of the `Build` step.