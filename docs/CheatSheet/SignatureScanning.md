# Signature Scanning

!!! info

    This page provides the basic essential background knowledge about signature scanning to spread awareness.  
    This technique is often underutilised by new and upcoming communities; games get updated and on some services, it's not possible to downgrade.  

    Please remember that the Cheat Sheet is ***not*** a game hacking guide.  

Modifying applications which get frequent updates can become very challenging. Every update shuffles around everything in the executables: functions, variables etc. Many other mods that don't use signature scanning for maintained games break instantly every update.  

This can be avoided with a technique called `signature scanning`.  

## About Signature Scanning

Signature Scanning is all about scanning memory to find a known sequence of bytes which matches a sequence of bytes you have previously identified (e.g. bytes at start of a function). That's it. 

Most common use cases for signature scanning:  
- Scanning for the start of a function.  
- Scanning for the code that accesses a variable.  

When doing Sigscanning, you should be looking for `CODE` and NOT `DATA`.  
You look for assembly instructions, such as `mov eax, [ecx]`. If you want to find e.g. the `money` variable, you scan for the code that accesses that address.  

## Creating Signatures

An example signature for getting address to `levelId` variable in a game.  

```csharp
// Signature: 89 15 [?? ?? ?? ??] EB ?? A3 ?? ?? ?? ??  
// LevelId  : Found Address of Signature + 2 (See brackets)  

// Instruction              // Bytes
mov levelId, edx            // 89 15 [?? ?? ?? ??] <= Level ID | 
jmp loc_4354A1 	            // EB ??
mov someOtherVariable, eax  // A3 ?? ?? ?? ??
```

When you are creating a signature, you want to exclude parts of the code that are likely to change between different versions of the game. This includes items such as `variable addresses` and `jmp targets`.  

In the above example, these bytes are marked as `??` and the signature scanner will accept any value in that position as valid.  

### Viewing bytes corresponding to instructions:  
- IDA: `Options` -> `General` -> `Number of Opcode Bytes`
- Ghidra: Ghidra shows bytes by default. 
- Binary Ninja (Cloud): Press dropdown for current tab and check `Show Bytes`.

### Hints on generating signatures by hand

!!! help
    The guidance here could be better.  

It's generally recommended to automate this process using existing scripts, however if you want to do this by hand; here is a bit of guidance:  
- You can generally tell by just seeing the desired address/offset right there in the bytes.  
- Using x86 instruction set reference can be useful to separate the instruction from operands.  
- Copy the code to an assembler like FASM, change operand, see which bytes change.  
- Ghidra Specific: `Right Click` -> `Instruction Info` -> See: `Operand Mask`.  

### Tool: SigMaker for IDA

!!! help
    There exist more tools for creating signatures out there.  
    If you are experienced with any of them, consider contributing to the wiki.  

[You can find SigMaker here](https://github.com/ajkhoury/SigMaker-x64). To install simply drop the plugin into the `plugins` directory. You can then access `SigMaker` via `Edit -> Plugins -> SigMaker` or by pressing `Ctrl + Alt + S`. 

A successfully found signature looks like `E8 ? ? ? ? 5B 5E 83 C4 18` and will be printed in the `Output` window.  

#### Direct Signature to Code
The following method can be used to generate a signature that when scanned, will give the exact address of the code you are looking for.  

- Highlight a line of code.  
- Generate a signature. (`Ctrl + Alt + S` -> `Create IDA Pattern from Selection`).  
- Test the pattern. (`Ctrl + Alt + S` -> `Test IDA Pattern`).  
- If there is more than 1 result, highlight 1 more line of assembly and repeat the process until there is only 1 result.  
- Use this signature.  

This generally generates longer signatures than `Auto Create Pattern`, but these signatures should be safer for cases where you are generating signatures for code in the middle of a function.  

#### Auto Create Pattern

If you decide to instead use `Auto Create Pattern` to generate the signature, you might receive an output that looks like this:  

```
direct reference: [actual address in first opcode] E8 ? ? ? ? 5B 5E 83 C4 18
```

And if you scan for the pattern, you might find yourself at a different address.
```
0060913F 024 E8 [7C 13 00 00]   call ChangeStrategyMode // <= Here 
00609144 024 5B                 pop  ebx
```

`Auto-create pattern` will often generate a pattern to some code that `call`(s) the function you are looking for.  
In order to convert this to the actual function address at runtime, you will need to do the following.  

- Extract the relative address [shown in brackets] from the `call instruction` (in this case bytes 0x7C130000).  
- Add this relative address to address of next instruction.  

```csharp
// Reference C# code to do this:  
// instructionAddress: Address of found signature in RAM.  
// instructionLength: Length of the instruction.
unsafe byte* CallInstructionToAbsoluteAddress(byte* instructionAddress, int instructionLength)
{
    var nextInstructionAddress = instructionAddress + instructionLength;
    var offset = *(instructionAddress + 1); // extract [ 7C 13 00 00 ]
    return (nextInstructionAddress + offset);
}
```

If the signature scan result says `sig to containing function`, add the number in brackets to your result. 

### Tool: MakeSig For Ghidra
[You can find MakeSig here](https://github.com/YaLTeR/ghidra_scripts).  

To install simply drop `makesig.py` into your `ghidra_scripts` directory.  

If you do not know where this directory is, you can check in `Window -> Script Manager` (or using the Script Manager button in the toolbar) then `Right Click -> Script Directories`.  

The default directory is `~/ghidra_scripts` although in this window you can add another directory where scripts will be searched for.  

With the script installed you can then use it from the `Script Manager` window by searching for `makesig` and double clicking it. You can also assign a keyboard shortcut to run the script by `Right Click -> Assign Key Binding`. After running it once you can also rerun it with `Ctrl + Shift + R` or using the rerun button at the end of the toolbar.  

When running the script you can either create a signature for the start of the function or the currently selected instruction, the found signature such as `E8 ?? ?? ?? ?? 8B 45 ?? 89 47` will be printed to the console.

## Searching for Signatures

!!! info

    The following example makes use of the [Reloaded.Memory.SigScan](https://www.nuget.org/packages/Reloaded.Memory.SigScan) NuGet library.  
    [You can find its documentation here](https://github.com/Reloaded-Project/Reloaded.Memory.SigScan).  

The following is an example of how you would search for the signature in the `Creating Signatures` section above:  

```csharp
// Initialize the scanner.
var thisProcess = Process.GetCurrentProcess();
var baseAddress = thisProcess.MainModule.BaseAddress;
var exeSize     = thisProcess.MainModule.ModuleMemorySize;
using var scanner = new Scanner((byte*)baseAddress, exeSize);

// Search for a given pattern
// Note: If created signature using SigMaker, replace ? with ??.
var result = scanner.FindPattern("89 15 ?? ?? ?? ?? EB ?? A3 ?? ?? ?? ??");
if (!result.Found)
    throw new Exception("Signature for getting LevelId not found.");
            
// Address of `mov levelId, edx`
var codeAddress = baseAddress + result.Offset;

// Address of `levelId` (skip 89 15)
var levelIdPointer = (int**)(codeAddress + 2);

// Get address of 'levelId' in memory.
int* levelId = *levelIdPointer;
```

## Additional Resources

- [Guided Hacking: Pattern Scanning Guide](https://guidedhacking.com/threads/external-internal-pattern-scanning-guide.14112/)  
- [AlliedMods: Pattern Scanning](https://wiki.alliedmods.net/Signature_Scanning)  
- [Reloaded Wiki](https://github.com/Sewer56/Reloaded.SharedLib.Hooks.ReloadedII#supporting-actively-developed-applications)  