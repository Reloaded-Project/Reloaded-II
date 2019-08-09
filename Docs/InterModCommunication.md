<div align="center">
	<h1>Reloaded II: Inter Mod Communication</h1>
	<img src="./Images/Reloaded/Reloaded Logo.png" width="150" align="center" />
	<br/> <br/>
	<strong>No DLL Exports? No problem!</strong>
	<br/>
    Providing a Reloaded-native implementation to a rare but sometimes useful feature.
</div>

# Table of Contents

- [Introduction](#introduction)
  - [Plugins and Controllers: Similarities and Differences](#plugins-and-controllers-similarities-and-differences)
    - [Sharing Interfaces (Important!)](#sharing-interfaces-important)
    - [Instantiation](#instantiation)
      - [Required Dependencies](#required-dependencies)
      - [Optional Dependencies](#optional-dependencies)
    - [Number of Instances](#number-of-instances)
    - [Disposal](#disposal)
      - [Rules (Simplified)](#rules-simplified)
        - [Storing Controllers](#storing-controllers)
        - [Using Controllers](#using-controllers)
  - [Using Plugins](#using-plugins)
    - [Instantiating](#instantiating)
  - [Using Controllers](#using-controllers)
    - [Registering](#registering)
    - [Acquiring](#acquiring)
    - [Disposing](#disposing)
  - [Recommendations & Limitations](#recommendations-limitations)
    - [Interface DLLs are Immutable](#interface-dlls-are-immutable)
    - [Keep all your Interfaces in a separate library](#keep-all-your-interfaces-in-a-separate-library)
    - [Upgrading Interfaces used with Controllers/Plugins](#upgrading-interfaces-used-with-controllersplugins)
  - [Examples](#examples)


# Introduction

Reloaded-II provides two mechanisms for what it considers "Inter Mod Communication", dubbed "Plugins and Controllers".

**Plugins** allow you to extend the functionality of other mods by implementing interfaces defined by them inside your mod. The mod loader API searches, creates and returns new instances of classes implementing a specific interface from other mods. *In other words, plugins do not share state.*

**Controllers** allow you to directly interact with other loaded mods. Individual mods can submit shared instances of interfaces to the mod loader, which can then be obtained by other mods. *Controllers share state.*

To make use of these, you first require an instance of the `IModLoader` interface, normally obtained as a parameter to your mod's entry point that allows for interaction with the mod loader.

## Plugins and Controllers: Similarities and Differences 

### Sharing Interfaces (Important!)

Before you get started with `Controllers` and `Plugins`, there is a very important piece of information that must be addressed.

If you are a developer of a mod which will share interfaces to be used by other mods, 
*you must first share the interface with the mod loader*. 

This can be done by inheriting the interface `IExports` (from `Reloaded.Mod.Interfaces`) in your main mod, with single method `GetTypes` which should return an array of interfaces to be consumed by other mods.

**Example:**
```csharp
public class Exports : IExports 
{
    public Type[] GetTypes() => new[] { typeof(IController) };
}
```

Sharing forces other mods to load the same instance of the DLL containing the shared interfaces (regardless of whether the version is newer or older!). If you do not share your interfaces other mods wouldn't be able to find it when making calls to the mod loader. 

**If you do not do this, `MakeInterfaces` and `GetController` will not work for other mods.**

### Instantiation

In order to be able to consume a **Plugin** or **Controller** from another mod, you must first set that mod's id as either a **required** or **optional** dependency.

This is a safeguard in order to ensure that unintended libraries do not get unified between otherwise isolated mod instances. This allows for the safe use of Shared Libraries..

#### Required Dependencies

(Mods specified as `ModDependencies` in `ModConfig.json`)

Reloaded-II re-sorts the order at which the mods are loaded on launch, guaranteeing that any required dependency of your mod will be loaded before your mod. As such, you are free to obtain Controllers and/or Plugins immediately in the entry point of your mod.

```csharp
IController _controller;
void Start(IModLoaderV1 loader) 
{
	_controller = loader.GetController<IController>();
}
```

#### Optional Dependencies

(i.e. Mods specified as `OptionalDependencies` in `ModConfig.json`)

If the mod is an optional dependency (i.e. not in the mod's dependencies list), then the preferred option is to acquire Controllers/Plugins after the mod loader has finished initializing (all mods are loaded).

To do this, simply subscribe to `IModLoader`'s `OnModLoaderInitialized`, and try to obtain an interface instance at that point.

```csharp
IModLoader _loader;
IController _controller;
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

*Unlike required dependencies, the mod loader does not take load order into account with optional dependencies.*

### Number of Instances
**Plugins** are considered a `0 - *` relationship, whereby a request for plugins which inherit from a specific interface may return anywhere between 0 to many plugins originating from various different mods.

**Controllers** are however considered a `0 - 1` relationship. Only one instance of a specific interface maximum can be registered at once into the mod loader at once (this simplifies the API).

### Disposal
Some Reloaded Mod Loader mods support live unloading from a running application.
However this poses a problem. If interfaces from a mod about to be unloaded is used by another mod, the mod cannot be fully unloaded.

Reloaded aims to alleviate this problem by returning both Plugins and Controllers as **Weak References**, i.e. instances of the `WeakReference<T>` class, and holding the actual strong references to the Plugins/Controllers themselves.

This means that when mods are unloaded from the process, the mod loader can dispose of them without the need of mod developers having manually doing so.

#### Rules (Simplified)

##### Storing Controllers

✅ Storing Weak References on the Heap is OK
```
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

❌  Storing referenced objects on the Heap is NOT OK.

```csharp
IController _controller;
void AcquireController() 
{
	_controller = _loader.GetController<IController>().Target;
    // This prevents the mod loader from being unable to dispose the controller.
}
```

##### Using Controllers

✅ Always check the controller is valid and hasn't been disposed before usage.

```csharp
void DoSomethingWithController() 
{
	if (_controller != null && 
        _controller.TryGetTarget(out var controller))
    {
    	// Do something with controller
    }
}
```

## Using Plugins

### Instantiating

Acquiring new instances of **Plugins** is performed through the use of the `MakeInterfaces` method.

```csharp
// _loader is an instance of IModLoader
WeakReference<ISharedInterface>[] interfaces = _loader.MakeInterfaces<ISharedInterface>();
```

## Using Controllers

For those who may be familiar with the concept of Dependency Inversion, then Reloaded-II acts similarly to an Inversion of Control (IoC) container of sorts. Reloaded stores manually submitted references/binds to objects by type, which can then be requested/resolved by other mods.

### Registering

To register a controller for it to then be used by other mods, simply use the `AddOrReplaceController` method of `IModLoader`. Once this is done, other mods may pick up this controller.

```csharp
Controller _controller;
void AddController() 
{
    _controller = new Controller(); // Implements IController
	_loader.AddOrReplaceController<IController>(this, _controller);
    // This refers to `IMod`, containing the mod's entry point.
}
```

### Acquiring

To acquire a controller that has been submitted from another mod, simply ask the mod loader.

```csharp
WeakReference<Controller> _controller;
void GetController() 
{
    _controller = _loader.GetController<IController>();
}
```

### Disposing

While the mod loader is capable of automatically disposing your controllers when your mod is unloaded, it should still be noted that it is possible to dispose and/or replace these manually with the `RemoveController` method.

```csharp
void Unload() 
{
    _loader.RemoveController<IController>();    
}
```

You may force a Garbage Collection with `GC.Collect()` to ensure other mods no longer have access to/acquire a new controller should you ever remove or override an existing controller.

## Recommendations & Limitations

### Interface DLLs are Immutable
Once a DLL containing interfaces has been loaded into the process, it cannot be unloaded.

e.g. If you recompile `SomeMod.Interfaces`, it cannot be swapped out without restarting the application/process. 

This is unfortunately a limitation of .NET Core, as DLLs cannot be unloaded from AssemblyLoadContext(s), only whole load contexts.

**Note:** This does not mean mods using the interface DLLs cannot be unloaded. Said mods can be loaded and unloaded during runtime as usual without any problems.

### Keep all your Interfaces in a separate library

Anyone using your interfaces will need to reference them from the project which contains the definitions.

*This means that if you store your interfaces in the same DLL as your main mod, others would need to reference your mod. Anyone using your interfaces will have your entire mod compiled and copied to their output! (Not good!)*

Therefore it is recommended that you make a separate library for storing only your shared interfaces. Both the main mod, and other mods using your **Controllers/Plugins** should reference that same library.

Ideally interface libraries should be 4-10KB and contain no external references (single DLL).

### Upgrading Interfaces used with Controllers/Plugins
Changes to existing interfaces will break mods using the interfaces. This should be fairly obvious to anyone who has used a plugin or a plugin-like system before.

If you want to add more functionality to existing interfaces, either make a new interface or extend the current interface via inheritance.

**Example:** Registering upgraded controller via inheritance:

```csharp
// Controller implements IControllerV2 which inherits IControllerV1.
ModLoader.AddOrReplaceController<IControllerV1>(this, _controller); ModLoader.AddOrReplaceController<IControllerV2>(this, _controller);
```

## Examples
The following examples contain mods that either export interfaces to be used by other mods and/or consume interfaces from other mods.

- [Reloaded Universal File Redirector](https://github.com/Reloaded-Project/Reloaded.Mod.Universal.Redirector)