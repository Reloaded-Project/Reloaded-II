<div align="center">
	<h1>Reloaded II: API Overview</h1>
	<img src="./Images/Reloaded/Reloaded Logo.png" width="150" align="center" />
	<br/> <br/>
	<strong>~Insert message worthy of cringe here~</strong>
</div>

# Table of Contents
- [Introduction](#introduction)
- [Variants of Communication](#variants-of-communication)
  - [Mod to Loader Communication](#mod-to-loader-communication)
  - [Mod to Mod Communication](#mod-to-mod-communication)
- [API Upgrades & Versioning](#api-upgrades-versioning)
  - [Backwards Compatibility: Mod & Loader Interfaces](#backwards-compatibility-mod-loader-interfaces)
- [Mod Loader Server](#mod-loader-server)
  - [Backwards Compatibility: Mod Loader Server](#backwards-compatibility-mod-loader-server)

# Introduction

In Reloaded II, in-process communication between mods and the loader as well as Inter Mod Communication (IMC) is performed mainly through the use of extensible interfaces that can be found in the `Reloaded.Mod.Interfaces` namespace.

`Reloaded.Mod.Interfaces` is contained and distributed as a tiny interface only library that contains various interfaces such as `IModLoader` or `IMod`. It is included by default in Reloaded mod projects and also available as a NuGet package. 

# Variants of Communication

This feature (the use of interfaces for communication) is entirely optional. Most mods will probably not need to make use of it - this is for more complex mods. 

That said, the use of the `Reloaded.Mod.Interfaces` is compulsory, as it provides the `IMod` interface used by the mod loader to find an entry point to execute user mods.

## Mod to Loader Communication

Mod to loader communication is used to provide various miniature mod loader services such as but not limited to:
- Querying the mod loader version.
- Writing to the console.
- Inter Mod Communication (IMC)
- Receiving events on mod load, unload etc.

The communication between the mod and the mod loader is handled by the `IModLoader` interface. `IModLoader` interface is immediately available to you in the entry point of your mods, as the `IModLoader` interface is provided as a parameter of the `Start` method of the `IMod` interface.

## Mod to Mod Communication

Reloaded's own Mod to Mod communication (Inter Mod Communication) is performed through the use of the aforementioned `IModLoader` interface that allows for interaction with the mod loader.

Reloaded-II provides two mechanisms for what it considers "Inter Mod Communication", dubbed "Plugins and Controllers".

**Plugins** allow you to extend the functionality of other mods by implementing interfaces defined by them inside your mod. The mod loader API searches, creates and returns new instances of classes implementing a specific interface from other mods. *In other words, plugins do not share state.*

**Controllers** allow you to directly interact with other loaded mods. Individual mods can submit shared instances of interfaces to the mod loader, which can then be obtained by other mods. *Controllers share state.*

Complete documentation on Mod to Mod communication can be found within the [Inter Mod Communication](./InterModCommunication.md) documentation.

# API Upgrades & Versioning

*Updates to the mod loader API as well as all general interfaces are expected to be very infrequent. Frequency will depend on developer requests however the estimate is less than once a year.*

## Backwards Compatibility: Mod & Loader Interfaces

As time progresses and new features are added, Reloaded-II needs to preserve compatibility with old mods using older interfaces, how does Reloaded-II plan to achieve this?

The answer to this specific question can be summed up with the word `inheritance`, specifically the inheritance of interfaces.

Specifically, Reloaded interfaces used for communication follow a strict set of rules:

- All versions of interfaces extend from the previous version.
- When passing interfaces between boundaries of different isolated units (e.g. Mod to Loader), the first version of the interface is always used.
- Receivers obtaining the interface promote it by casting to the newest available version known to them.

```csharp
public static void Start(IModLoaderV1 loader) 
{
	_loader = (IModLoader)loader;
    // IModLoader is an empty interface that extends from the most recent version, e.g. IModLoaderV3
    // This allows for auto-upgrading interface by updating NuGet package without changing base code.
}
```


# Mod Loader Server

Reloaded-II also supports out-of-process communication through the use of a locally hosted server inside each process which has loaded Reloaded.

The data structures, list of server calls and responses of the server can be found in the separate `Reloaded.Mod.Loader.Server` project. That said, this server is only intended for internal use by the launcher to allow for status reporting and basic mod management.

## Backwards Compatibility: Mod Loader Server
Backwards compatibility at server end for requests is handled in the following simple way:

- The Server accepts all available requests available in the most recent version of Reloaded-II, regardless of whether it may be always supported.
- If a given mod-related request is not supported by a mod, the Server will return a `ReloadedException` with an accompanying error message. Example:`"Feature unavailable. Interface IModV2 is not supported by this mod."`.