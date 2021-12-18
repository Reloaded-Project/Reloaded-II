# Prerequisites

- .NET 6.0 SDK (or newer).
- Visual Studio 2019+
	- "Desktop development with C++" workload (Windows 10 SDK)
	
# Obtaining Reloaded
- Clone this repository and fetch the submodules.

```
git clone https://github.com/Reloaded-Project/Reloaded-II
cd Reloaded-II
git submodule update --init --recursive
```

# Building Reloaded

- Open 'Source/Reloaded-II.sln' in Visual Studio.
- Build.

*You're done (\* ^ ω ^), go play around!*

# Releasing Reloaded

To distribute Reloaded, build it using the publish script `Publish.ps1`.

1. Search for and open `Developer Command Prompt for VS 2019`.
2. Change current directory to the `Source` directory.
3. Open Powershell by typing in `powershell`.
4. Execute `.\Publish.ps1`.

The output should reside in the `Publish` folder.