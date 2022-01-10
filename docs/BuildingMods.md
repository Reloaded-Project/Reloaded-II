# Building Mods

## Prerequisites

- .NET SDK (6.0 or newer)  
- Visual Studio 2017+ [Recommended]  

## Project from Template

### 1. Import the Visual Studio Template
The template, named `Reloaded II Mod Template.zip` can be found in the root of this repository.

To add it to Visual Studio, copy the zip to `C:\Users\[User Name]\Documents\[Visual Studio Version]\Templates\Project Templates`. 

You may now create a new project.
![New Project](./Images/NewProject.png)

*(If the project(s) do not show up, this is a Visual Studio bug. Close Visual Studio, open the Visual Studio developer command prompt and run `devenv /updateConfiguration`.)*

### 2. Update the Mod Configuration and Image

Once imported, update the `ModConfig.json` file with the relevant details of your project. 

![Config](./Images/JsonFile.png)

You can edit this file by hand or alternatively from within the launcher.  

Fields of interest:  
- `ModName`: The name of your mod.  
- `ModAuthor`: The author of your mod.  
- `ModDescription`: A description of your mod.  
- `SupportedAppId`: Applications for which the mod should be enabled for by default. Uses lower case exe name.  

To edit within the launcher, do the following:  
- Build (in Visual Studio).  
- Launcher: `Manage Mods` -> Select `Reloaded II Mod Template` -> `Edit Mod`.  
- Open Mod Folder, and copy the `ModConfig.json` file back to your source code.  

Then proceed to replace the mod preview image `Preview.png`.

### 3. Building And Debugging

Simply build your project in Visual Studio. It should show in the `Manage Mods` tab of Reloaded Launcher and your game tab (if you have added it to `SupportedAppId` in `ModConfig.json`).  
 
![Debugger Launch](./Images/DebuggerLaunch.png)

When building your mod in `Debug` configuration, you should automatically receive a debug prompt like the following when running your game/application.   
Select your currently open version of Visual Studio and you're free to debug.  

### 4. Publishing

Please refer to [Publishing Mods](`./PublishingMods.md`) for the latest instructions for publishing mods.

## Creating Project From Scratch 

If you have no access to Visual Studio or would rather not use a template, you can create a new project from scratch.  
Please [use the following guide](./ProjectFromScratch.md) to create a mod project from scratch.  
