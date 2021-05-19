# NuGet Sources

NuGet is the primary source for downloading mods and searching for missing mod dependencies.
You can directly download mods from NuGet servers via the `Download Mods` menu.

Reloaded comes preconfigured with a single [official server](http://packages.sewer56.moe:5000/home) intended for hosting code mods.

## Adding new Sources

Additional NuGet servers can be configured from inside the `Download Mods` menu by pressing `Configure Sources`.

![](./Images/NewNugetFeed.png)

Simply press the `New` button and then fill a name and URL. The URL should point to something called a `NuGet Index`. This is a URL which generally ends with `/v3/index.json`.

This URL is generally provided on either a `Feed Details` or `Upload Instructions` page.

Example URLs:

- Official NuGet Gallery Index: [https://api.nuget.org/v3/index.json](https://api.nuget.org/v3/index.json)
- Example MyGet Index URL: [https://www.myget.org/F/reloaded-ii-tests/api/v3/index.json](https://www.myget.org/F/reloaded-ii-tests/api/v3/index.json)
- Official BaGet Server URL: [http://packages.sewer56.moe:5000/v3/index.json](http://packages.sewer56.moe:5000/v3/index.json)

------------
**Please Note:**

Reloaded handles NuGet errors silently. If it cannot contact a NuGet server; it wouldn't display any error message, etc.

------------

## Hosting A Server

Reloaded-II uses the NuGet V3 API and as such any API compliant NuGet server should work correctly without problems. The official Reloaded server uses a modified version of [BaGet](https://github.com/loic-sharma/BaGet), forked as [BaGet-Reloaded](https://github.com/Sewer56/BaGet-ReloadedII). Self hosting instructions for a clean VPS are provided in the repository.

Other servers/services known to work properly are [MyGet](https://www.myget.org) as well as the official [NuGet Gallery](https://github.com/NuGet/NuGetGallery).