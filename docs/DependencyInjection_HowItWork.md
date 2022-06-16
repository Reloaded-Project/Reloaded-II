# Introduction

Sometimes you might want to check the state of another mod, run some code from it and/or instruct another mod to perform a certain action. Reloaded-II provides a mechanism to do this.  

## How Does it Work?

Implemented using interfaces, the concept is that the mod loader acts as a middleman between mods (it's a DI container!). This middleman allows mods to communicate by passing implementations of interfaces between each other.  

This may be illustrated by the following diagram:  

![Example](./Diagrams/Images/InterModCommunication.png)

*An example of an external mod communicating with the [Universal File Redirector](https://github.com/Reloaded-Project/reloaded.universal.redirector).*  

- During initialization, Mod A (Redirector) publishes an interface to the Mod Loader.  
- During initialization, Mod B (Other Mod) asks the Mod Loader for the interface.  

Communication with the Mod Loader is performed using the `IModLoader` interface, available at your mod's entry point.  