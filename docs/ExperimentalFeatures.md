# Experimental & Miscellaneous Features

This page lists Reloaded features which don't necessarily fit into other pages of the wiki.

## Experimental Features

Experimental features are features which are either partially implemented, or which have not yet been thoroughly tested.  
Use at your own risk.

### Portable Mode

Reloaded II can be put into portable mode by creating a file called `portable.txt` in the same directory as the launcher.  

When portable mode is used, Reloaded will always use the `Apps`, `Mods`, `User` and `Plugins` folders from the launcher's directory.  

Other global settings still apply, however.  

### Relative Pathed Applications

It's possible to set a relative `AppLocation` for programs, as opposed to a fixed path.  
This will allow you to move the launcher to a subfolder of a game should you plan to only use Reloaded with one game, or ship the launcher preconfigured for a given game.  

Example Configuration (`Apps/sonicriders.exe/AppConfig.json`):  

```json
// `Reloaded II/Apps/sonicriders.exe` -> `Sonic Riders/SonicRiders.exe`
{
  "AppId": "sonicriders.exe",
  "AppName": "Sonic Riders",
  "AppLocation": "..\\..\\..\\Sonic Riders\\SonicRiders.exe",
  "AppArguments": "",
  "AppIcon": "Icon.png",
  "AutoInject": false
}
```

All paths are relative to the folder in which the `AppConfig.json` file is located.  

## Miscellaneous Features

### RGB Window Border

You can set your window border to hue cycle through the colors of the rainbow.  
This is a hidden feature that can be used in themes, and is set in `Theme/Default/Settings.xaml`.  

Property name is `EnableGlowHueCycle`.