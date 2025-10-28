//#define TEST_MODE

using Reloaded.Mod.Loader.Update.Providers.GameBanana.Structures;

namespace Reloaded.Mod.Launcher.Utility;

/// <summary>
/// Manages updating the list of available themes, and downloading/installing them
/// </summary>
public class ThemeDownloader
{
    private static List<GameBananaMod> AvailableThemes = [];

    // Things like my theme pack contain multiple themes in one (bad idea in hindsight), so now I have to account for that -zw
    /// <summary>
    /// The key is the name of a subtheme contained in a pack, the value is the index into AvailableThemes
    /// </summary>
    public static Dictionary<string, int> ThemesDictionary = [];

    /// <summary>
    /// Called after RefreshAvailableThemes runs
    /// </summary>
    public static Action? OnRefresh = null;

    private static string GetFileNameFromModName(string modName)
    {
        // If using the mod's name as the theme name, it will remove 'theme' from the name, fix any double spaces that might cause, trim it, and then replace spaces with underscores
        return modName.Replace("Theme", "").Replace("theme", "").Replace("  ", " ").Trim().Replace(" ", "_");
    }

#if TEST_MODE
    private static GameBananaModFile TestModFile(string filename, string description, string url)
    {
        var file = new GameBananaModFile();
        file.FileName = filename;
        file.Description = description;
        file.DownloadUrl = url;
        return file;
    }

    private static GameBananaMod TestMod(string name, string description, List<GameBananaModFile> files)
    {
        var mod = new GameBananaMod();
        mod.Name = name;
        mod.Description = description;
        mod.Files = files;
        return mod;
    }

    // A manual re-creation of the results the API call should get back, so I can test it out for sort-of real
    private static void TestFetch()
    {
        var discordTheme = TestMod(
                            "Discord Theme",
                            "A theme that makes Reloaded II look and feel like Discord.",
                            [ TestModFile("discord.zip", "", "https://gamebanana.com/dl/489094") ]
        );

        var personaTheme = TestMod(
                            "Persona 4 Golden theme",
                            "since P4 is gonna get a update and make RII become even more outdated (for this game), i decided to make this as a goodbye.\ni tried to change the font, but it won't delete for some reason.",
                            [ TestModFile("p4g_theme.zip", "", "https://gamebanana.com/dl/886552") ]
        );

        var heroesTheme = TestMod(
                            "heroes mod loader theme",
                            "this mod simply replaces the logo with a custom heroes mod loader one.",
                            [ TestModFile("default_5073e.zip", "", "https://gamebanana.com/dl/634883"), TestModFile("reloadednobrackets.zip", "", "https://gamebanana.com/dl/635018") ]
        );

        var themePack = TestMod(
                            "Zack's Theme Pack Vol. 1",
                            "These are my set of themes for Reloaded<br><br><ul class=\"SelectedElement\"><li>GreenApple</li><li>Orange</li><li>Teal</li><li>Violet</li><li>Midnight</li><li>Monochrome</li><li>Metallic Default</li><li>Metallic Silver</li></ul><br>I'll probably make more in the future so I'm calling this volume 1<br><br><h2>Installing</h2>To install it, copy the contents of the zip folder to the Theme folder wherever Reloaded is installed. The themes will show up in the drop down the next time Reloaded is launched.",
                            [ TestModFile("zacksthemepackvol1.zip", "", "https://gamebanana.com/dl/1546487") ]
        );
        AvailableThemes = [discordTheme, personaTheme, heroesTheme, themePack];
    }
#endif

    /// <summary>
    /// Fetches all themes from GameBanana, and updates the dictionary
    /// </summary>
    /// <param name="fetch">Whether or not to fetch from the server, if not it just updates the drop down with what it's already got</param>
    public static async Task RefreshAvailableThemes(bool fetch=true)
    {
        if (fetch)
        {
            ThemesDictionary.Clear();
            AvailableThemes.Clear();

#if TEST_MODE
            TestFetch();
#else
            // Hangs here forever
            AvailableThemes = await GameBananaMod.GetByNameAsync("", 7486, 1, 5, "GUIs");
#endif

            for (int i = 0; i < AvailableThemes.Count; i++)
            {
                var theme = AvailableThemes[i];
                if (theme.Name == null) continue;

                // It checks for a bullet-point list to determine which themes are contained in a pack
                if (theme.Description != null && theme.Description.Contains("<ul"))
                {
                    // Prepare for my trademark python text manipulation -zw
                    var startIndex = theme.Description.IndexOf("<ul");
                    startIndex = theme.Description.IndexOf(">", startIndex) + 1;
                    var endIndex = theme.Description.IndexOf("</ul>");
                    var containingThemes = theme.Description[startIndex..endIndex].Replace("<li>", "").Replace(" ", "_").Replace("\n", "").Split("</li>")[..^1];
                    foreach (var subtheme in containingThemes)
                    {
                        if (subtheme != "")
                            ThemesDictionary.Add(GetFullPath(subtheme + ".xaml"), i);
                    }
                }
                else
                    ThemesDictionary.Add(GetFullPath(GetFileNameFromModName(theme.Name) + ".xaml"), i);
            }
        }

        OnRefresh?.Invoke();
    }

    private static readonly string ThemeFolder = "Theme";
    public static readonly string TempFolder = "Theme/.tmp";
    public static readonly string TempZip = "Theme/tmptheme.zip";

    /// <summary>
    /// Turns a .xaml filename into the full absolute path for the merged dictionary
    /// </summary>
    /// <param name="filename">The .xaml from the file selector</param>
    public static string GetFullPath(string filename)
    {
        string path = Path.GetFullPath($"{ThemeFolder}/{filename}");

        // I think the double slash is a bug, I'm recreating it so it works
        path = path.Replace($"Launcher{Path.DirectorySeparatorChar}", $"Launcher{Path.DirectorySeparatorChar}{Path.DirectorySeparatorChar}");

        return path;
    }

    private static readonly string[] ThemeContentsFilenames = [
        "Colours.xaml",
        "Controls.xaml",
        "CustomStyles.xaml",
        "Images.xaml",
        "Styles.xaml"
    ];

    // For safety, the downloader is not allowed to overwrite the default themes
    // Special exceptions have been made for the already existing themes, where they are converted to a standalone theme.
    private static readonly string[] InvalidMoveLocations = [
        $"{ThemeFolder}/Default",
        $"{ThemeFolder}/Default.xaml",
        $"{ThemeFolder}/Halogen",
        $"{ThemeFolder}/Halogen.xaml",
        $"{ThemeFolder}/Helpers",
        $"{ThemeFolder}/NoCorners.xaml"
    ];

    private static void CheckMoveDir(string destination)
    {
        if (InvalidMoveLocations.Contains(destination))
            throw new InvalidDataException("Cannot install themes that modify the default ones");
    }

    private static void MoveTempFiles(string destination)
    {
        CheckMoveDir(destination);

        foreach (var file in Directory.GetFiles(TempFolder))
        {
            var newFile = $"{destination}/{Path.GetFileName(file)}";
            CheckMoveDir(newFile);
            File.Move(file, newFile);
        }
            
        foreach (var folder in Directory.GetDirectories(TempFolder))
        {
            var newFolder = $"{destination}/{Path.GetFileName(folder)}";
            CheckMoveDir(newFolder);
            Directory.Move(folder, newFolder);
        }
            
    }

    private static void DeleteTempFiles()
    {
        if (File.Exists(TempZip))
            File.Delete(TempZip);

        if (Directory.Exists(TempFolder))
            Directory.Delete(TempFolder, true);
    }

    private static void CopyFromHalogen(string src, string dst, string themeName)
    {
        File.WriteAllText(dst, File.ReadAllText(src).Replace("Halogen", themeName));
    }

    private static bool ThemeNeedsToBeCreated(out List<bool> existingXAMLs)
    {
        bool createTheme = false;

        existingXAMLs = [];
        for (int i = 0; i < ThemeContentsFilenames.Length; i++)
            existingXAMLs.Add(false);

        string[] files = Directory.GetFiles(TempFolder);
        foreach (var file in files)
        {
            var name = Path.GetFileName(file);
            if (ThemeContentsFilenames.Contains(name))
            {
                // The zip contains the contents of a theme, so a new theme needs to be made from it
                createTheme = true;
                existingXAMLs[ThemeContentsFilenames.IndexOf(name)] = true;
            }
        }

        return createTheme;
    }

    private static void DownloadTheme(GameBananaMod theme)
    {
        // There needs to be a standard for how to upload themes
        // I'll support all of the current ones but there's likely to be an edge case that causes it to break in the future -zw

        if (theme.Files == null || theme.Files.Count == 0)
            return;

        // These should be deleted at the end but just in case it crashes or something
        DeleteTempFiles();

        string themeName = GetFileNameFromModName(theme.Name!);
        string themeFolder = $"{ThemeFolder}/{themeName}";

        if (theme.Files.Count == 1 && theme.Files[0].FileName.EndsWith(".zip"))
        {
            new ThemeDownloadDialog(theme.Files[0]);

            if (ThemeNeedsToBeCreated(out var existingXAMLs))
            {
                Directory.CreateDirectory(themeFolder);
                MoveTempFiles(themeFolder);

                // Checking for the persona 4 golden theme
                if (theme.Files[0].FileName == "p4g_theme.zip")
                {
                    Directory.Move($"{themeFolder}/R-II/Images", $"{themeFolder}/Images");
                    File.Move($"{themeFolder}/R-II/Settings.xaml", $"{themeFolder}/Settings.xaml");
                    Directory.Delete($"{themeFolder}/R-II", true);
                }

                CopyFromHalogen($"{ThemeFolder}/Halogen.xaml", themeFolder + ".xaml", themeName);
                // For any XAMLs that don't exist, it'll copy the one from halogen and replace the name with the theme's name so the folder directories point to the correct place
                for (int i = 0; i < existingXAMLs.Count; i++)
                {
                    if (!existingXAMLs[i])
                        CopyFromHalogen($"{ThemeFolder}/Halogen/{ThemeContentsFilenames[i]}", $"{themeFolder}/{ThemeContentsFilenames[i]}", themeName);
                }
            }
            else
                MoveTempFiles(ThemeFolder);
        }
        // Checking for the heroes mod loader theme
        else if (theme.Files.Count == 2 && theme.Files[0].FileName == "default_5073e.zip")
        {
            new ThemeDownloadDialog(theme.Files[0]);

            Directory.Move($"{TempFolder}/Default", themeFolder);
            Directory.Delete(TempFolder);

            new ThemeDownloadDialog(theme.Files[1]);

            string imagesFolder = $"{themeFolder}/Images";
            Directory.Move(TempFolder, imagesFolder);

            File.Move($"{themeFolder}/R-II/Controls.xaml", $"{themeFolder}/Controls.xaml");
            CopyFromHalogen($"{ThemeFolder}/Halogen/CustomStyles.xaml", $"{themeFolder}/CustomStyles.xaml", themeName);
            File.WriteAllText($"{themeFolder}/Images.xaml", File.ReadAllText($"{themeFolder}/R-II/Images.xaml").Replace("Default/R-II/Images/R", $"{themeName}/Images/R"));
            Directory.Delete($"{themeFolder}/R-II", true);
            Directory.Delete($"{themeFolder}/Fonts", true);
            CopyFromHalogen($"{ThemeFolder}/Halogen.xaml", themeFolder + ".xaml", themeName);
        }

        DeleteTempFiles();
    }

    /// <summary>
    /// Finds the theme from the given .xaml, and then downloads and installs it
    /// </summary>
    /// <param name="name">The .xaml to load from the theme selector</param>
    /// <param name="viewModel">The view model for the download window, so it can update the progress bar and text as it goes</param>
    public static void DownloadThemeByName(string name)
    {
        foreach ((var subtheme, var index) in ThemesDictionary)
        {
            if (subtheme == name)
            {
                DownloadTheme(AvailableThemes[index]);
                break;
            }
        }
    }
}
