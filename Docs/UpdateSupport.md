<div align="center">
	<h1>Reloaded II: Mod Updates</h1>
	<img src="./Images/Reloaded/Reloaded Logo.png" width="150" align="center" />
	<br/> <br/>
	<strong>Simple, but effective!</strong>
	<br/>
    Also quite fast!
</div>

# Table of Contents
- [Supported Archive Formats](#supported-archive-formats)
- [Existing Services](#existing-services)
    - [Reloaded II Repository](#reloaded-ii-repository)
    - [Github](#github)
        - [Determining Version](#determining-version)
        - [User Configuration](#user-configuration)
        - [Limitations](#limitations)
    - [GameBanana](#gamebanana)
        - [File Name Pattern](#file-name-pattern)
        - [Item Type](#item-type)
        - [Item Id](#item-id)
        - [Determining Version](#determining-version)
- [Programmers: New Services](#programmers-new-services)
    - [1. Write a Resolver](#1-write-a-resolver)
    - [2. Add resolver to list of Resolvers](#2-add-resolver-to-list-of-resolvers)

# Supported Archive Formats

See: [SharpCompress Supported Format Table](https://github.com/adamhathcock/sharpcompress/blob/master/FORMATS.md#supported-format-table)

# Existing Services

**Note:** When the mod loader checks for updates, it will only use maximum one of the compatible sources below.

The mod loader goes through each of the sources, listed in the exact same order as sections below and picks the first appropriate service which is supported.

This is to speed up the time it takes to check for updates, as asking every source for every mod could otherwise take significant amount of time.

### Reloaded II Repository

**Support:** A package is considered as "supported" if it is uploaded to this service.

This is a self hosted official service for distributing Reloaded-II mods. When any dependencies of a given mod are missing, this is also the repository used for resolving dependencies. 

This repository is open to everyone: Registration free!

To upload: first generate a NuGet package for your mod from inside the launcher using the `Convert to NuGet format` button.

![Example](https://i.imgur.com/V7uq4Jl.png)

Once conversion completes, Reloaded will open a folder containing the newly generated `.nupkg` file.

To upload this file, use the `dotnet` commandline utility that ships with the .NET Core SDK (if you are using Reloaded-II you'll probably have this).

```
dotnet nuget push -s http://167.71.128.50:5000/v3/index.json -k YOUR-UNIQUE-KEY package.nupkg
```

Before you upload however, you **must** read the [Upload Page](http://167.71.128.50:5000/upload) for some additional important information.

Please be respectful and also follow the [Site Rules](http://167.71.128.50:5000/home).

### Github

**Support:** A package is considered as "supported" if the file below exists.

Support for mod updates from Github Releases can be added by copying the `ReloadedGithubUpdater.json` file from the Launcher's `Template` folder the to mod folder.

After copying, you should then edit the file to include the user/organization name, repository and name of the mod archive.

*Example:*

```json
{
  "UserName": "Reloaded-Project",
  "RepositoryName": "Reloaded.Mod.Universal.Redirector",
  "AssetFileName": "Mod.zip"
}
```

##### Determining Version
To determine the current version of the mod, the Github service uses the `ModVersion` field inside of your mod's `ModConfig.json`.

To determine the version on Github, the Github service uses the `tag` assigned to each release. 

If the Github version is higher than the local one, there is an update.

##### User Configuration

Each mod with Github update support can be configured by the user. This can be done by editing the `ReloadedGithubUserConfig.json`  file, in the mod folder using a standard text editor. 

**Example file:**
```json
{
  "LastCheckTimestamp": 0,
  "EnablePrereleases": false
}
```

If not present, this file will appear the next time the Reloaded Launcher is launched.

**Note:** *ReloadedGithubUserConfig.json should not be included in any mod downloads!*

##### Limitations
- Prereleases are supported but semantic versioning is not. Please do not add any prefixes/suffixes to your release tags.

- Github only allows 60 requests an hour for unauthenticated users. This means that if you have many mods with Github update support, they might not receive updates immediately.
	- The Github service tracks when each mod has been checked, ensuring that each mod gets the chance to check for updates, even if there are more than 60.

### GameBanana

**Support:** A package is considered as "supported" if the file below exists.

Support for mod updates from GameBanana can be added by copying the `ReloadedGamebananaUpdater.json` file from the Launcher's `Template` folder the to mod folder.

After copying, you should then edit the file to include the entry ID and type.

*Example:*

```json
{
  "FileNamePattern": "rii-",
  "ItemType": "Skin",
  "ItemId": 162715
}
```

##### File Name Pattern
The `FileNamePattern` is a piece of text which the name of the uploaded file (media) must contain to be recognized as the correct file upload. The default is `rii-`.

It is used to distinguish the Reloaded II download from downloads that might come in other formats, e.g. for other mod loaders.

**File Pattern Guidelines:**
✅ Piece of text at the beginning (preferred) or end of the file name.
✅ Use a pattern that will only match the name of one file.
❌ Do not include file extensions in your pattern.

Gamebanana adds suffixes to duplicate file names, even if original file is removed. So if you upload `rii-midnighthill.7z`, remove it, and reupload it again, the file might be named something like `rii-midnighthill_eaa22.7z`.

**Warning**
- Some characters are removed/replaced when uploading files to GameBanana.
- Long file names (~32+ characters) might get trimmed down.

The text comparison used by the GameBanana service is case insensitive.

##### Item Type
A list of item types can be obtained from the following page: [GameBanana API Item Types](https://api.gamebanana.com/Core/Item/Data/AllowedItemTypes?). 

The item type should match the submission type of your mod. The type of submission of your mod can be found in the URL in plural form.

e.g. `gamebanana.com/skins/162715`=> `Skin`

e.g. `gamebanana.com/gamefiles/7104`=> `Gamefile`

##### Item Id
The Item ID of your submission can be obtained from the URL of your submission: 
![](https://i.imgur.com/WQBjezg.png)

In order to obtain your Item ID, it is recommended that you first upload your mod as private: 
![](https://i.imgur.com/o1lvS4n.png)

##### Determining Version
To compare versions of the mod, the GameBanana service uses the `Upload Date` of the and compares it against the last modified date of your mod's `ModConfig.json`.

If the GameBanana upload is more recent than the last edit of `ModConfig.json`, a new update will be reported.

# Programmers: New Services

Here is a simple rundown of how to add a new service for a 3rd party website.

### 1. Write a Resolver
Inside the `Resolvers` folder of the `Reloaded.Mod.Loader.Update` project, write a class that implements the `IModResolver` class.

**Example:** See `GithubLatestUpdateResolver`.

The class only needs to be able to determine if there's a newer version, Reloaded-II will always autoselect newest version to update regardless of how many updates are available.

### 2. Add resolver to list of Resolvers

Inside `ResolverFactory`, add your new resolver to the `Resolvers` array inside `ResolverCollection`.