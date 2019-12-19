<div align="center">
	<h1>Reloaded II: Optimizing Mods</h1>
	<img src="./Images/Reloaded/Reloaded Logo.png" width="150" align="center" />
	<br/> <br/>
	<strong>.NET is like an 86.<br/>Slow start but good round the corners.</strong>
</div>

# Optimizing Mods

This document details various methods and techniques in which mods may be optimized to run for the end user. 

Focusing mainly on techniques which may not be immediately obvious, the main goal is to obtain better startup times, although some other optimizations are also available.

## Performance-Focused Optimizations

#### Lazy Loading

Lazy loading is simply the process of deferring initialization of an object/thing until the point at which it is needed.

Sometimes in your `Start()` entry point, you might not need to necessarily need to initialize everything related to your mod immediately.

For example, if you need to make a connection to a server which will be used later in execution (say you are interacting with a chatroom in a mod etc.), the initial startup of the mod does not have to be halted until this connection is made. You can make the connection in the background and any code depending on the connection can wait for the connection task to finish.

##### Lazy Loading Example

Consider a case where you have a code that requires an object, and the object is not required to be used in the `Start()` method itself.

You can create the object just in time for when it is needed:

```csharp
// Function hook by the game/application.
public void SomeHookFunction(int a1, int b2) 
{
	// Create someObject is not previously created.
	if (someObject == null)
		someObject = new SomeClass();
	
	/* Do something with someObject ... */
	
	// Call original function, as part of standard hook procedure
	_someHook.OriginalFunction(a1, b2);
}
```

This is a very simplified example.

The framework built-in class `Lazy<T>` is very useful. Consider reading this useful resource: https://docs.microsoft.com/en-us/dotnet/framework/performance/lazy-initialization

##### Asynchronous Loading

Another great way to not stall mod startup is to perform setup/initialization tasks asynchronously, the *Task Parallel Library* (TPL) is great for doing this.

Consider creating a connection to a server.

```csharp
// Inside class fields.
private Task _connectToServerTask;

// Inside Start()
_connectToServerTask = Task.Run(() => { /* Code to Connect to a Server */ });

// Somewhere else in code that needs the server to be created.
// (Unlikely for this specific scenario).
_connectToServerTask.Wait(); // Will stall execution until task completes.
```

##### Danger: Race Conditions

The important thing to note about lazy loading however is the possibility of race conditions for shared resources (such as native memory). Mainly, please **do not create function hooks asynchronously**. 

Consider two mods creating hooks for the same function at the same time. When they read the first initial bytes of the function into memory, they would get the same result for the 'original function' to execute. 

What does this mean? The hooks are unaware of each other (as 'original code'). Calling the 'original function' from either of the hooks would not call the other hooks. 

In other words, only one of the mods' hooks works.

#### Publish as ReadyToRun

.NET Core 3 has a new type of officially supported file format for publishing applications known as *ReadyToRun* (abbreviated as R2R).

The main advantage of R2R is that it boasts **significant** improvement to startup times by shipping native code alongside IL code to essentially create hybrid assemblies.

For more details, see [Ready to Run Guide](./ReadyToRun.md).

#### Use Shared Libraries

See [Shared Libraries](https://github.com/Sewer56/Reloaded.SharedLib.Hooks#table-of-contents) doc. for the benefits, drawbacks, etc.

## Space-Focused Optimizations

#### Update Target Framework
The individual Reloaded Mod Templates target .NET Standard 2.0 (at the time of writing) by default. This however does not mean you have to necessarily target `netstandard2.0`.

You are free however to target newer versions of .NET Standard or even .NET Core directly, as long as the version of Standard/Core is supported by the version of Core the loader was built with.

To do so, simply edit your project's `.csproj` file:

```xml
<TargetFramework>netcoreapp3.0</TargetFramework>
```

*The target DLL is now built for NET Core 3.*

Often for complex mods, targeting .NET Core directly results in smaller build sizes, as many of the APIs such as `System.Text.Json` are directly available in Core, rather than fetched from NuGet and added to the output of your mod.

