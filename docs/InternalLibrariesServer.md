# Server Library

!!! info

    Library for communicating with 'Reloaded II Server' mod (formerly part of mod loader).  
    Allows for remote querying of information as well as mod manipulation (e.g. load/unload mods) in real time.  
    [[NuGet Package]](https://www.nuget.org/packages/Reloaded.Mod.Loader.Server)

## Connect to Server

!!! info

    Connects to the server for remote control.

```csharp
// Create a new client. will automatically asynchronously connect.  
// Can specify remote IP address, but user must enable 'Allow External Connections' in 'Reloaded II Server' Mod.  
// ... and port forward
Client = new LiteNetLibClient(IPAddress.Loopback, "", _port, true);

// If the server restarts, e.g. user unloaded and reloaded the mod, 
// you should probably try to get the server's new port number (in case it changed).
Client.OnTryReconnect += (peer) => TryGetPort(out _port); // see below how to get new port
Client.OverrideDetailsOnReconnect += () => (null, _port); // overrides port in reconnect attempt

// Report back exceptions.
Client.OnReceiveException += ClientOnReceiveException;

// Useful:
//  Client.OnConnected | When client connects to host.
//  Client.IsConnected | If client is connected.
```

If the server is hosted on the same machine, you can get the port number programmatically:  
```csharp
ServerUtility.GetPort(processId);
```

## Check if Reloaded is Present

!!! info

    Checks if Reloaded has been loaded into a process with a given ID.

```csharp
ReloadedMappedFile.Exists(processId);
```