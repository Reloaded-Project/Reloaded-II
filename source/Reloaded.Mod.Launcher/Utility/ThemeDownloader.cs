//#define TEST_MODE

using Reloaded.Mod.Loader.Update.Providers.GameBanana.Structures;
using MessageBox = Reloaded.Mod.Launcher.Pages.Dialogs.MessageBox;

namespace Reloaded.Mod.Launcher.Utility;

/// <summary>
/// Manages updating the list of available themes, and downloading/installing them
/// </summary>
public class ThemeDownloader
{
    private static List<GameBananaMod> AvailableThemes = [];

    /// <summary>
    /// The key is the name of a subtheme contained in a pack, the value is a 32-bit index into available themes concatenated with a 32-bit file index
    /// </summary>
    public static Dictionary<string, ulong> ThemesDictionary = [];

    /// <summary>
    /// Called after RefreshAvailableThemes runs
    /// </summary>
    public static Action? OnRefresh = null;

    // If using the mod's name as the theme name, it will remove 'theme' from the name, fix any double spaces that might cause, trim it, and then replace spaces with underscores
    private static string GetFileNameFromModName(string modName) => modName.Replace("Theme", "").Replace("theme", "").Replace("  ", " ").Trim().Replace(" ", "_");

    private static ulong CombineIndices(int themeIndex, int fileIndex) => ((ulong)themeIndex << 32) | (uint)fileIndex;

    private static void SeparateIndices(ulong index, out int themeIndex, out int fileIndex)
    {
        themeIndex = (int)(index >> 32);
        fileIndex = (int)(index & uint.MaxValue);
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
            int attempts = 0;
        Retry:
            try
            {
                AvailableThemes = await GameBananaMod.GetByNameAsync("theme", 7486, 1, 50);
            }
            catch (Exception e)
            {
                if (attempts++ < 10)
                    goto Retry;

                var messageBox = new MessageBox("Fetch Error", "Failed to fetch the mods! Check your internet connection, and if that's good, it might just be GameBanana's servers acting up, try again later");
                messageBox.ShowDialog();

                return;
            }
#endif

            for (int themeIndex = 0; themeIndex < AvailableThemes.Count; themeIndex++)
            {
                var theme = AvailableThemes[themeIndex];
                if (theme.Name == null) continue;

                switch (theme.Files![0].FileName)
                {
                    case "default_5073e.zip":
                        ThemesDictionary.Add(GetFullPath("heroes_mod_loader.xaml"), CombineIndices(themeIndex, 0));
                        continue;

                    case "p4g_theme.zip":
                        ThemesDictionary.Add(GetFullPath("Persona_4_Golden.xaml"), CombineIndices(themeIndex, 0));
                        continue;

                    // Unfortunately even the discord theme needs an exception here since the zip name isn't capitalized like the xaml
                    case "discord.zip":
                        ThemesDictionary.Add(GetFullPath("Discord.xaml"), CombineIndices(themeIndex, 0));
                        continue;

                    default:
                        for (int fileIndex = 0; fileIndex < theme.Files.Count; fileIndex++)
                        {
                            if (theme.Files[fileIndex].FileName.EndsWith(".zip"))
                                ThemesDictionary.Add(GetFullPath(theme.Files[fileIndex].FileName[..^4] + ".xaml"), CombineIndices(themeIndex, fileIndex));
                        }
                        break;
                }
            }
        }

        OnRefresh?.Invoke();
    }

    private static readonly string ThemeFolder = "Theme";
    public static readonly string TempFolder = $"{ThemeFolder}/.tmp";
    public static readonly string TempZip = $"{ThemeFolder}/tmptheme.zip";

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

    private static async Task DownloadTheme(GameBananaMod theme, string themeName, int fileIndex, ThemeDownloadDialog dialog)
    {
        // The standard is 1 theme per zip file
        // I'll still support all of the current ones as of writing this though

        if (theme.Files == null || theme.Files.Count == 0)
            return;

        // These should be deleted at the end but just in case it crashes or something
        DeleteTempFiles();

        string themeFolder = $"{ThemeFolder}/{themeName}";

        GameBananaModFile file = theme.Files[fileIndex];

        // It would make more sense to have it be == and then return, but I want the diff to be as clean as possible -zw
        if (file.FileName != "default_5073e.zip")
        {
            if (await dialog.DownloadAndExtractZip(file) != ThemeDownloadDialogResult.Ok)
                goto Exit;

            if (ThemeNeedsToBeCreated(out var existingXAMLs))
            {
                Directory.CreateDirectory(themeFolder);
                MoveTempFiles(themeFolder);

                // Checking for the persona 4 golden theme
                if (theme.Files[fileIndex].FileName == "p4g_theme.zip")
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
        else
        {
            if (await dialog.DownloadAndExtractZip(theme.Files[0]) != ThemeDownloadDialogResult.Ok) goto Exit;

            Directory.Move($"{TempFolder}/Default", themeFolder);
            Directory.Delete(TempFolder);

            if (await dialog.DownloadAndExtractZip(theme.Files[1]) != ThemeDownloadDialogResult.Ok) goto Exit;

            string imagesFolder = $"{themeFolder}/Images";
            Directory.Move(TempFolder, imagesFolder);

            File.Move($"{themeFolder}/R-II/Controls.xaml", $"{themeFolder}/Controls.xaml");
            CopyFromHalogen($"{ThemeFolder}/Halogen/CustomStyles.xaml", $"{themeFolder}/CustomStyles.xaml", themeName);
            File.WriteAllText($"{themeFolder}/Images.xaml", File.ReadAllText($"{themeFolder}/R-II/Images.xaml").Replace("Default/R-II/Images/R", $"{themeName}/Images/R"));
            Directory.Delete($"{themeFolder}/R-II", true);
            Directory.Delete($"{themeFolder}/Fonts", true);
            CopyFromHalogen($"{ThemeFolder}/Halogen.xaml", themeFolder + ".xaml", themeName);
        }

    Exit:
        dialog.Close();
        DeleteTempFiles();
    }

    /// <summary>
    /// Finds the theme from the given .xaml, and then downloads and installs it
    /// </summary>
    /// <param name="name">The .xaml to load from the theme selector</param>
    public static async Task DownloadThemeByName(string name, ThemeDownloadDialog dialog)
    {
        foreach ((var theme, var index) in ThemesDictionary)
        {
            if (theme == name)
            {
                SeparateIndices(index, out var themeIndex, out var fileIndex);
                await DownloadTheme(AvailableThemes[themeIndex], Path.GetFileName(name)[..^5], fileIndex, dialog);
                break;
            }
        }
    }
}
