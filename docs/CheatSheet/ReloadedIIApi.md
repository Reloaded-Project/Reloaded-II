# Reloaded API

Reloaded-II provides a simple, easy to use API that can be used for receiving various events from the mod loader, query status of other mods and perform some common operations such as logging.  

## Where to find the API.

In the mod template, have a look at `Program.cs`.  
When your mod first executes at startup, Reloaded will call the `Start` (or `StartEx`) method.  

You should subscribe to any events that you want to receive from Reloaded-II in this method.  

```csharp
public void StartEx(IModLoaderV1 loaderApi, IModConfigV1 modConfig)
{
    // Previous code omitted..
    // From template
    _mod = new Mod(_hooks, _logger);

    // Ideally here, at the end of the method.
}
```

## Some Common API Events

```csharp
// Subscribing to events from `StartEx` method.
public void StartEx(IModLoaderV1 loaderApi, IModConfigV1 modConfig)
{
    // Prior code omitted from this secion...
    var configFolder = _modLoader.GetModConfigDirectory(modConfig.ModId);
    _modLoader.OnModLoaderInitialized += OnModLoaderInitialized;
    _modLoader.ModUnloading += ModUnloading;
    _modLoader.ModLoading += OnModLoading;
    _modLoader.ModLoaded += OnModLoaded;
}

// Called when all the mods have been loaded at startup.
private void OnModLoaderInitialized() { }

// Called right before a mod is unloaded.
private void ModUnloading(IModV1 mod, IModConfigV1 config) { }

// Called right before a mod is loaded.
private void OnModLoading(IModV1 mod, IModConfigV1 config) { }

// Called after a mod is loaded
private void OnModLoaded(IModV1 mod, IModConfigV1 config) { }
```

When consuming events, the parameters passed to the event handlers contain the oldest available versions of each interface.  
To use the newest available version, simply cast the interface.

```csharp
var newMod = (IMod) mod
```

Newest versions have no version suffix, so for `IModConfigV1` you should cast to `IModConfig` and for `IModV1` you should cast to `IMod` etc.

## Logging API

Reloaded-II provides a logging API; which you can use to log messages to the console and the log file.  

Example:  
```csharp
_logger.WriteLineAsync("Hello World");
```

### Asynchronous Logging

If you need to very frequently log messages, it is recommended to use the asynchronous logging API instead.  

```csharp
_logger.WriteLineAsync("Async Hello World");
```

Calling this method will add the message to a queue.  
This queue is handled by a background thread; so the method will return immediately instead of stalling the application.  

### Events

The logger exposes some events, such as for intercepting logged messages. 

```csharp
_logger.OnWriteLine += LoggerOnOnWriteLine;

private void OnWriteLine(object? sender, (string text, Color color) e)
{
    /* Do something with new line. */
}
```

This can be useful if you would like to e.g. display the log in-game. 