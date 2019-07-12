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

# Introduction

Reloaded-II provides two mechanisms for what it considers "Inter Mod Communication", dubbed "Plugins and Controllers".

**Plugins** allow you to extend the functionality of other mods by implementing interfaces defined by them inside your mod. The mod loader API searches, creates and returns new instances of classes implementing a specific interface from other mods. *In other words, plugins do not share state.*

**Controllers** allow you to directly interact with other loaded mods. Individual mods can submit shared instances of interfaces to the mod loader, which can then be obtained by other mods. *Controllers share state.*

To make use of these, you first require an instance of the `IModLoader` interface, normally obtained as a parameter to your mod's entry point that allows for interaction with the mod loader.

## Plugins and Controllers: Similarities and Differences 

### Instantiation

#### Required Dependencies

Reloaded-II re-sorts the order at which the mods are loaded on launch, guaranteeing that any required dependency of your mod will be loaded before your mod. As such, you are free to obtain Controllers and/or Plugins immediately in the entry point of your mod.

```csharp
IController _controller;
void Start(IModLoaderV1 loader) 
{
	_controller = loader.GetController<IController>();
}
```

#### Optional Dependencies

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
	if (! _controller.IsAlive) 
    {
    	// Re-acquire controller or exit the method. 
    } 
    
	// Do something with controller
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