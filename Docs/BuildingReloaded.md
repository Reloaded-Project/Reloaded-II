<div align="center">
	<h1>Reloaded II: Building</h1>
	<img src="./Images/Reloaded/Reloaded Logo.png" width="150" align="center" />
	<br/> <br/>
	<strong>CringyQuotes.Count == 0</strong>
	<br/>
    I'm out of ideas for something to put here.
    <br/>
    Feel free to add something here with a pull request.
</div>

# Table of Contents

- [Table of Contents](#table-of-contents)
- [Prerequisites](#prerequisites)
- [Obtaining Reloaded](#obtaining-reloaded)
- [Building Reloaded](#building-reloaded)
- [Releasing Reloaded](#releasing-reloaded)

# Prerequisites

- Visual Studio 2019+
- Visual Studio: "Desktop development with C++" (Windows 10 SDK)
- .NET Core 3.0 Preview 6 SDK (or newer).
- NuGet Package Manager (Fetching Dependencies)

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

To distribute Reloaded, simply build it as shown above.
After building, run the batch script  `CleanForDeployment.cmd` which can be found alongside `Reloaded-II.sln`.

You may then zip up and distribute the `Launcher` folder inside `Output`, that contains everything you need.

