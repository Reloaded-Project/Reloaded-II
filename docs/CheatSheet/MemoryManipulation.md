# Memory Manipulation

!!! warning

    The `Cheat Sheet` is a work in progress.  
    If you think it can be improved, please message me or contribute to the wiki.  

!!! info

    The goal of the `Cheat Sheet` is to provide information to help you work within a Reloaded environment.  
    It is not a game hacking/reverse engineering guide, but rather a way to help you if you are unfamiliar working with C# or Reloaded.  

This page provides various common use case examples of memory manipulation with Reloaded.  

## Using Pointers

!!! info

    When using pointers in C#, you must use the `unsafe` keyword, which you can apply to a class, method or block of code.

Classic and the most commonly known method of manipulating memory, C style pointers.

```csharp
public unsafe void PointerExample()
{
    int* ptr = (int*)0xA0B8C0;

    // Set 32-bit value at 0xA0B8C0 to 1.
    *ptr = 1;

    // Read back the value from 0xA0B8C0
    int value = *ptr;
}
```

## Using Structs

Pointer arithmetic is also possible with structs in C#.  

```csharp
public struct Point
{
    public float X;
    public float Y;
}

public unsafe void PointerStructExample()
{
    var ptr = (Point*)0xA0B8C0;

    // Set X coordinate to 2000.0f
    ptr->X = 2000.0f;
}
```

### Using Structs (Custom Offsets)

C# allows you to set custom offsets for fields inside structs.  

```csharp
[StructLayout(LayoutKind.Explicit)]
public struct Player
{
    [FieldOffset(0x24)]
    public byte State;

    [FieldOffset(0x100)]
    public Vector3 Position;
}
```

There is no need to insert dummy values as you might need to do in other languages.  
These structs work fine with pointers and other C# functionality.  

## ASLR

Although rarely, some games might use ASLR (Address Space Layout Randomization) to randomize the address where the contents of the game's executable is loaded in memory.  

To get the base address of the EXE in memory, use `Process.GetCurrentProcess().MainModule.BaseAddress`.

```csharp
// baseAddress: location where the 1st byte of the EXE resides in memory
var baseAddress = (long) Process.GetCurrentProcess().MainModule.BaseAddress;
var functionAddress = baseAddress + 0x4D6920;
```

When you see, for example `Tsonic_win.exe+4D6920` in a tool like Cheat Engine, it really means means `baseAddress + 0x4D6920`.  

## Using the Reloaded.Memory Library

!!! info

    The following examples make use of the [Reloaded.Memory](https://www.nuget.org/packages/Reloaded.Memory) NuGet library.  

    If you are unfamiliar with NuGet, please use one of the following guides:  
    - [Commandline (dotnet)](https://docs.microsoft.com/en-us/nuget/quickstart/install-and-use-a-package-using-the-dotnet-cli)  
    - [Visual Studio](https://docs.microsoft.com/en-us/nuget/quickstart/install-and-use-a-package-in-visual-studio)  
    - [Rider](https://www.jetbrains.com/help/rider/Using_NuGet.html)

Below are some common `Reloaded.Memory` use cases related to Memory Manipulation, for more utilities, please consult the [Reloaded.Memory documentation](https://github.com/Reloaded-Project/Reloaded.Memory/blob/master/Docs/Getting-Started.md).

### Simple Memory Read/Write

```csharp
public void ReloadedMemoryExample()
{
    var memory = Memory.Instance; // in Reloaded.Memory.Sources

    // Set 32-bit value at 0xA0B8C0 to 1.
    memory.Write(0xA0B8C0, 1);

    // Read back the value from 0xA0B8C0
    int value = memory.Read<int>(0xA0B8C0);
}
```

### Protected Memory Regions

Sometimes, sections of memory may be protected such that writing to them using or the regular `Write` methods will cause an error. When this happens you will usually see an error that looks like `Access Violation (0xC0000005)`.  

This usually happens when you make attempts to directly overwrite game code. `Reloaded.Memory` provides a workaround for this.  

```csharp
public void ReloadedMemorySafeWrite()
{
    // SafeWrite will change memory permissions, 
    // write your value and restore the original permissions
    Memory.Instance.SafeWrite(0xA0B8C0, 1);
}
```

Alternatively, you can use `ChangePermission` to change the permissions and never bother with them again.  

```csharp
public void ReloadedMemoryChangePermissions()
{
    var value  = 1;
    var memory = Memory.Instance;

    // Change the permissions so this region of memory is writable.
    memory.ChangePermission((IntPtr)0xA0B8C0, ref value, Kernel32.MEM_PROTECTION.PAGE_EXECUTE_READWRITE);

    // You can now write to the memory as usual.
    memory.Write(0xA0B8C0, value);
}
```

### Pointers by Reference

The `RefPointer<T>` class can be used to convert pointers to references.  

```csharp
public unsafe void RefPointerExample()
{
    ref int value = ref RefPointer<int>.Create(0xA0B8C0);
    value = 1;        // Write to 0xA0B8C0
    int copy = value; // Read from 0xA0B8C0
}
```

### Fixed Array Pointers

The `FixedArrayPointer<T>` struct is also worth a mention, as it implements the necessary interfaces to make use of features such as LINQ.

```csharp
// Assume 0xA0B8C0 contains an array of character physics (struct) in some game.
// Assume there are 16 characters.
var characterPhysicsArray = new FixedArrayPtr<CharacterPhysics>((ulong)0xA0B8C0, 16);

// Calculating min, max, average air acceleration using LINQ
float minAccel = characterPhysicsArray.Min(physics => physics.AirAcceleration);
float maxAccel = characterPhysicsArray.Max(physics => physics.AirAcceleration); 
float averageAccel = characterPhysicsArray.Average(physics => physics.AirAcceleration);
```

### Memory Marshalling Example

"Marshalling" is another feature supported in `Reloaded.Memory` when reading or writing data.  
Marshalling is the process of converting between a C# object and a C struct or C++ class.  
The following example reads a struct, which contains a string with fixed length.  

```csharp
public struct CustomFileHeader
{
    /// <summary>
    /// Contains the actual filename of the file as ASCII encoded bytes.
    /// Stored as fixed size of 32 bytes, no null terminator.
    /// </summary>
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
    public string Name;

    public uint Offset;
    public uint Length;
}

private static void MarshallingExample()
{
    var memory = Memory.Instance;

    // Read. 
    memory.Read<CustomFileHeader>(memoryLocation, out var header, true); // true to enable marshalling
    
    // Change the name (make sure less than 32 characters)
    header.Name = "CoolNewHeaderName";
    
    // Write it back
    memory.Write(memoryLocation, customHeader);
}
```