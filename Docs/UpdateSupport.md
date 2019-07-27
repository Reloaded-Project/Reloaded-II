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
    - [Github](#github)
        - [Determining Version](#determining-version)
        - [User Configuration](#user-configuration)
        - [Limitations](#limitations)
- [Programmers: New Services](#programmers-new-services)
    - [1. Write a Resolver](#1-write-a-resolver)
    - [2. Add resolver to list of Resolvers](#2-add-resolver-to-list-of-resolvers)

# Supported Archive Formats

See: [SharpCompress Supported Format Table](https://github.com/adamhathcock/sharpcompress/blob/master/FORMATS.md#supported-format-table)

# Existing Services

### Github

Support for mod updates from Github Releases can be added by copying the `ReloadedGithubUpdater.json` file from the Launcher's `Template` folder the to mod loader folder.

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

# Programmers: New Services

Here is a simple rundown of how to add a new service for a 3rd party website.

### 1. Write a Resolver
Inside the `Resolvers` folder of the `Reloaded.Mod.Loader.Update` project, write a class that implements the `IModResolver` class.

**Example:** See `GithubLatestUpdateResolver`.

The class only needs to be able to determine if there's a newer version, Reloaded-II will always autoselect newest version to update regardless of how many updates are available.

### 2. Add resolver to list of Resolvers

Inside `ResolverFactory`, add your new resolver to the `Resolvers` array inside `ResolverCollection`.