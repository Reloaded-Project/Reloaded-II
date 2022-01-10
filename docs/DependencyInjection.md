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

## Usage: Consumer

### Prerequisites

Make sure you add the `ModId` of the mod you are consuming instances/interfaces from in the `ModDependencies` field of your `ModConfig.json`.

```csharp
"ModDependencies": [
  "reloaded.sharedlib.hooks",
  "Riders.Controller.Hook"
]
```

You can also do this from within the launcher (`Edit Mod`) and copy the new `ModConfig.json` back to your source code.  

*Reloaded will not allow mods to consume types from other mods without explicit opt-in*, this is a safeguard to ensure no unintended types are shared between mods (ensuring isolation).

### Consuming Dependencies

Use the `IModLoader` API to get an instance of the interface you want to consume.  
This interface should be available at the entry point (`Start`) of the mod you are consuming.

```csharp
WeakReference<Controller> _controller;
void GetController() 
{
    _controller = _loader.GetController<IController>();
}
```

✅ Always check the controller is valid and hasn't been disposed before usage.  
```csharp
void DoSomethingWithController() 
{
    // If the mod we got IController from is unloaded, `TryGetTarget` will fail.
	if (_controller != null && _controller.TryGetTarget(out var controller))
    {
        // Do something with `controller`
    }
}
```

### Life Cycle & Disposal

`*Note: If the publisher is not unloadable, you can ignore this section.*`

Some Reloaded mods support live, real time loading and unloading.  
As such, you must be careful with how you consume interfaces from other mods.  

In order to ensure unloading of publishers correctly happens, Reloaded uses "Weak References" (`WeakReference<T>`).  
This also means that when mods are unloaded, the Mod Loader can dispose of them cleanly without explicit code in other mods.  

Here are some guidelines on consuming interfaces:  

✅ Storing Weak References on the Heap is OK
```csharp
WeakReference<IController> _reference;
void AcquireController() 
{
	_reference = _loader.GetController<IController>();
}
```

✅ Storing referenced objects on the Stack is OK
```csharp
void AcquireController() 
{
	IController controller = _loader.GetController<IController>().Target;   
    // controller is no longer referenced outside of the scope of the method.
}
```

❌ Storing referenced objects on the Heap is NOT OK.
```csharp
IController _controller;
void AcquireController() 
{
	_controller = _loader.GetController<IController>().Target;
    // This prevents the mod loader from being unable to dispose the controller.
}
```

## Usage: Publisher

### Prerequisites

If you are developing a mod which will share interfaces to be used by other mods, *you must first share the interface with the Mod Loader*. 

This is done by inheriting the interface `IExports` (from `Reloaded.Mod.Interfaces`) in your main mod, with single method `GetTypes` which should return an array of interfaces to be consumed by other mods.

**Example:**

```csharp
public class Exports : IExports 
{
    public Type[] GetTypes() => new[] { typeof(IController) };
}
```

Sharing forces other mods to load the same instance of the DLL containing the shared interfaces, regardless of whether the version is newer or older. 

**If you do not do this, `GetController` will not work for other mods.**

### Publishing Dependencies

For controllers an additional step is required. During initialization (`Start()`), you need to register it with the mod loader. Simply use the `AddOrReplaceController` method of `IModLoader`.

```csharp
Controller _controller;

_controller = new Controller(); // Implements IController
_loader.AddOrReplaceController<IController>(this, _controller);
```

### Disposing (Publisher)

The Mod Loader is capable of automatically disposing your dependencies when your mod is unloaded, however you can still manually (if desired) dispose and/or replace your dependency instances with the `RemoveController` method.

```csharp
void Unload() 
{
    _loader.RemoveController<IController>();    
}
```

## Recommendations & Limitations

### Interface DLLs are Immutable
***Once a DLL containing interfaces has been loaded into the process, it cannot be unloaded.***

e.g. If you recompile `SomeMod.Interfaces`, it cannot be swapped out without restarting the application/process.  

This is unfortunately a limitation of .NET (Core), as DLLs cannot be unloaded from `AssemblyLoadContext`(s), only whole load contexts.

**Note:** This does not mean mods using the interface DLLs cannot be unloaded. Said mods can be loaded and unloaded during runtime as usual without any problems.

### Keep all your Interfaces in a separate library

***Interface libraries should contain no external, 3rd party references*** (single DLL).

There are two issues:  

1. If you store your interfaces in the same DLL as your main mod.  
    - Not Good: Consumers using them will your entire mod compiled and copied to their output! (Not Good!)  

1. Sharing an interface shares its whole assembly, and all of the assemblies' dependencies.  
    - This can cause mods to unintentionally be forced to use another mod's dependencies.  

Therefore it is recommended that you make a separate library for storing only your shared interfaces. Both the main mod (producer), and other mods (consumer) should reference that same library.

### Upgrading Interfaces
Changes to existing interfaces will break mods using the interfaces. This should be fairly obvious to anyone who has used a plugin or a plugin-like system before.

If you want to add more functionality to existing interfaces, either make a new interface or extend the current interface via inheritance. After being published (Mod Released), interfaces should never change (this includes method names).

Here is an example of a possible setup:

```csharp
// Recommended setup using inheritance.
// User can acquire `IController` and have the latest version known at compile time.
interface IController : IControllerV3 { /* Dummy */ } 

interface IControllerV3 : IControllerV2 { void DoZ(); }
interface IControllerV2 : IControllerV1 { void DoY(); }
interface IControllerV1 { void DoX(); }
```

## Example Mods

The following examples contain mods that export interfaces to be used by other mods.

**Universal Mods**

- [Reloaded Universal File Redirector](https://github.com/Reloaded-Project/Reloaded.Mod.Universal.Redirector)
    - Producer: `Reloaded.Universal.Redirector`
    - Contract: `Reloaded.Universal.Redirector.Interfaces`
    - Consumer(s): `Reloaded.Universal.Monitor`, `Reloaded.Universal.RedirectorMonitor`

**Application Specific**

- [Sonic Heroes Controller Hook](https://github.com/Sewer56/Heroes.Controller.Hook.ReloadedII) (Allows other mods to receive/send game inputs.)
    - Producer: `Riders.Controller.Hook`
    - Contract: `Riders.Controller.Hook.Interfaces`
    - Consumer(s): `Riders.Controller.Hook.Custom`, `Riders.Controller.Hook.XInput`, `Riders.Controller.Hook.PostProcess`

- [Sonic Riders Controller Hook](https://github.com/Sewer56/Riders.Controller.Hook) (Allows other mods to receive/send game inputs.)
    - Producer: `Heroes.Controller.Hook`
    - Contract: `Heroes.Controller.Hook.Interfaces`
    - Consumer(s): `Heroes.Controller.Hook.Custom`, `Heroes.Controller.Hook.XInput`, `Heroes.Controller.Hook.PostProcess`

**Libraries as Dependencies**

- [PRS Compressor/Decompressor](https://github.com/Sewer56/Reloaded.SharedLib.Csharp.Prs.ReloadedII)
- [Reloaded.Hooks (Function Hooking/Detour Library)](https://github.com/Sewer56/Reloaded.SharedLib.Hooks.ReloadedII)

## Extra Features

### Factories

Reloaded's API also provides a functionality to create instances implementing a type. These are sometimes also referred to as `"Plugins"` in older docs. 
Acquiring new instances of **Plugins** is performed through the use of the `MakeInterfaces` method.

```csharp
// _loader is an instance of IModLoader
var interfaces = _loader.MakeInterfaces<ISharedInterface>();
```

When you run the above code, Reloaded will iterate through every loaded mod and create an instance of every class implementing `ISharedInterface`.  
The result of this operation (all instances) will be returned as an array.

### Optional Dependencies

(`OptionalDependencies` in `ModConfig.json`)

Mods can also use the `OptionalDependencies` field instead of the `ModDependencies` field. 
If the mod is an optional dependency, then the preferred option is to acquire Controllers/Plugins after the Mod Loader has finished initializing (all mods are loaded).

To do this, simply subscribe to `IModLoader`'s `OnModLoaderInitialized`, and try to obtain an interface instance at that point.

```csharp
IModLoader _loader;
WeakReference<IController> _controller;
void Start(IModLoaderV1 loader) 
{
   _loader = (IModLoader)loader;
   _loader.OnModLoaderInitialized += Initialized;
}

// Called by the mod loader after all mods finished loading.
void Initialized() 
{
	_controller = _loader.GetController<IController>();
}
```

*Unlike required dependencies, the Mod Loader does not take load order into account with optional dependencies.*

## How does it *Really* Work?

Reloaded makes extensive use of [AssemblyLoadContext](https://docs.microsoft.com/en-us/dotnet/core/dependency-loading/understanding-assemblyloadcontext)(s) to provide isolation between mods, such that they may use 3rd library plugins and dependencies of their choice without interfering with other mods. This can make it difficult to share state between a mod and the loader, and even more so between individual mods.  

Reloaded overcomes this challenge by sharing instances of `Assemblies` (DLLs), loosening up isolation around shared code. Mods can nominate which `Assemblies` they wish to share with other mods, and the loader will load them into a shared `AssemblyLoadContext`. When the mod is loaded, the Mod Loader will then force it use the shared `Assembly` instances from the shared `AssemblyLoadContext`.  

For publishers its own shared types are automatically shared with itself, for consumers this is determined using the `ModDependencies` field in `ModConfig.json`.  
![Example](./Diagrams/Images/InterModCommunication-Internal.png)  

### How is this arrangement Setup?

The whole process for sharing an `Assembly` looks something like this: 

1. Gathering Dependencies  
    1. Mod Loader loads `Redirector` (Publisher).  
    2. Mod Loader loads all shared Assemblies into Shared `AssemblyLoadContext`  
    3. Mod Loader unloads `Redirector` (Publisher).  

2. Loading Mods  
    1. Mod Loader loads `Redirector`, sharing its own `Assemblies` with itself (from 1.b).  
    2. Mod Loader loads `RedirectorMonitor` (Consumer).  
        1. `RedirectorMonitor` specified `Redirector` in its `ModDependencies` field.  
        2. Therefore Mod Loader shares all of `Redirector`'s exports with `RedirectorMonitor`.  
        
3. Executing Mods  
    1. Mod Loader rearranges mod load order to account for mod dependencies.  
    1. `Redirector` code executes. Redirector publishes `IRedirectorController` (from Reloaded.Mod.Interfaces.IRedirector.dll) to Mod Loader.  
    2. `Monitor` code executes. Monitor obtains `IRedirectorController` from Mod Loader.  