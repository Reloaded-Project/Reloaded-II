# Extra Features

## Interface Factories

!!! warning

    This feature has never been battle tested.  
    It's been Reloaded's least used feature since release; no known mod uses this at time of writing.  

Reloaded's API also provides a functionality to create all instances implementing a type.  
You can do this through the `MakeInterfaces` method.

```csharp
// _loader is an instance of IModLoader
var interfaces = _loader.MakeInterfaces<ISharedInterface>();
```

When you run the above code, Reloaded will iterate through every loaded mod and create an instance of every class implementing `ISharedInterface`.  
The result of this operation (all instances) will be returned as an array.  

## Optional Dependencies

(`OptionalDependencies` in `ModConfig.json`)

Mods can also use the `OptionalDependencies` field instead of the `ModDependencies` field.  
Doing so allows you to consume dependencies from the other package, but does not re-order the mod load order or give any warnings to the user.  

As such, you should call `GetController`/`MakeInterfaces` from the `OnModLoaderInitialized` event (called when all mods are loaded).

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