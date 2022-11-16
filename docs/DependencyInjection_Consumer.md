# How to Consume Dependencies

!!! note

    A specific mod load order is ***not*** required when consuming dependencies.  
    Reloaded will automatically rearrange load order when required to ensure dependencies are loaded before their consumers without affecting the order of other mods.  
    
## Set a Dependency on the Other Mod

In your `ModConfig.json`, update the `ModDependencies` to include the id of the mod you are consuming.  

```csharp
"ModDependencies": [
  "reloaded.sharedlib.hooks",
  "Riders.Controller.Hook"
]
```

## Add Other Mod's Interfaces Library to your Project

Every mod that publishes an interface will have something called an `Interfaces` library, which contains a collection of all the interfaces that the mod publishes.  

By convention, this library is usually named `<ModId>.Interfaces` and comes in the form of a NuGet package, for example:  
- [Reloaded Hooks Shared Lib](https://github.com/Sewer56/Reloaded.SharedLib.Hooks.ReloadedII) [(NuGet)](https://www.nuget.org/packages/Reloaded.SharedLib.Hooks)  
- [Memory SigScan Shared Lib](https://github.com/Reloaded-Project/Reloaded.Memory.SigScan)  [(NuGet)](https://www.nuget.org/packages/Reloaded.Memory.SigScan.ReloadedII.Interfaces)  

## Consume Dependencies

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

## Life Cycle & Disposal

!!! info

    Some Reloaded mods support real-time loading and unloading.  
    As such, you must be careful with how you consume interfaces from other mods.  
    
!!! note

    You can find out if a mod is unloadable by launching your game and clicking
    the process in the Processes list in Reloaded. If the unload button is not 
    greyed out, the mod is unloadable.  

    If the mod is not unloadable, you can ignore this section.  

In order to ensure unloading of publishers can correctly happen in the runtime, Reloaded uses "Weak References" (`WeakReference<T>`).  
Here is guidance on how to use them:  

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