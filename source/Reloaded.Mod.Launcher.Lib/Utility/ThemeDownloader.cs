using Reloaded.Mod.Loader.Update.Providers.GameBanana.Structures;

namespace Reloaded.Mod.Launcher.Lib.Utility;

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

    /// <summary>
    /// Fetches all themes from GameBanana, and updates the dictionary
    /// </summary>
    public static async Task RefreshAvailableThemes(bool fetch=true)
    {
        if (fetch)
        {
            ThemesDictionary.Clear();
            AvailableThemes.Clear();

            // Hangs here forever
            AvailableThemes = await GameBananaMod.GetByNameAsync("", 7486, 1, 5, "GUIs");

            for (int i = 0; i < AvailableThemes.Count; i++)
            {
                var theme = AvailableThemes[i];
                if (theme.Name == null) continue;

                if (theme.Description != null && theme.Description.Contains("<ul>"))
                {
                    // Prepare for my trademark python text manipulation -zw
                    var startIndex = theme.Description.IndexOf("<ul>") + 4;
                    var endIndex = theme.Description.IndexOf("</ul>");
                    var containingThemes = theme.Description[startIndex..endIndex].Replace("<li>", "").Replace(" ", "").Replace("\n", "").Split("</li>")[..^1];
                    foreach (var subtheme in containingThemes)
                    {
                        if (subtheme != "")
                            ThemesDictionary.Add(subtheme + ".xaml", i);
                    }
                }
                else
                    ThemesDictionary.Add(GetFileNameFromModName(theme.Name) + ".xaml", i);
            }
        }

        OnRefresh?.Invoke();
    }

    private static readonly string ThemeFolder = "Theme";
    private static readonly string TempFolder = "Theme/.tmp";
    private static readonly string TempZip = "Theme/tmptheme.zip";

    private static async Task DownloadAndExtractZip(GameBananaModFile file)
    {
        Stream data = await SharedHttpClient.UncachedAndCompressed.GetStreamAsync(file.DownloadUrl);
        var zipFile = File.Create(TempZip);
        data.CopyTo(zipFile);
        zipFile.Close();
        data.Close();

        ZipFile.ExtractToDirectory(TempZip, TempFolder);
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
        $"{ThemeFolder}/Helpers"
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
            Directory.Delete(TempFolder);
    }

    private static async Task DownloadTheme(GameBananaMod theme)
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
            await DownloadAndExtractZip(theme.Files[0]);

            bool createTheme = false;

            List<bool> existingXAMLs = new();
            for (int i = 0; i < 5; i++)
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

            if (createTheme)
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

                File.WriteAllText(themeFolder + ".xaml", File.ReadAllText($"{ThemeFolder}/Halogen.xaml").Replace("Halogen", themeName));
                // For any XAMLs that don't exist, it'll copy the one from halogen and replace the name with the theme's name so the folder directories point to the correct place
                for (int i = 0; i < 5; i++)
                {
                    if (!existingXAMLs[i])
                        File.WriteAllText($"{themeFolder}/{ThemeContentsFilenames[i]}", File.ReadAllText($"{ThemeFolder}/Halogen/{ThemeContentsFilenames[i]}").Replace("Halogen", themeName));
                }
            }
            else
                MoveTempFiles(ThemeFolder);
        }
        // Checking for the heroes mod loader theme
        else if (theme.Files.Count == 2 && theme.Files[0].FileName == "default_5073e.zip")
        {
            await DownloadAndExtractZip(theme.Files[0]);
            Directory.Move($"{TempFolder}/Default", themeFolder);
            Directory.Delete(TempFolder);

            await DownloadAndExtractZip(theme.Files[1]);
            string imagesFolder = $"{ThemeFolder}/{themeName}/Images";
            Directory.CreateDirectory(imagesFolder);
            Directory.Move(TempFolder, imagesFolder);
        }

        DeleteTempFiles();
    }

    /// <summary>
    /// Finds the theme from the given name, and then downloads and installs it
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public static async Task DownloadThemeByName(string name)
    {
        foreach ((var subtheme, var theme) in ThemesDictionary)
        {
            if (subtheme == name)
            {
                await DownloadTheme(AvailableThemes[theme]);
                break;
            }
        }
    }
}
