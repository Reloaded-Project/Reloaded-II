namespace Reloaded.Mod.Launcher.Utility;

public class XamlThemeSelector : XamlFileSelector
{
    public static readonly string FetchText = "Fetch Themes...";

    private void InsertFetchButton()
    {
        Files.Insert(0, FetchText);
    }

    public XamlThemeSelector(string directoryPath) : base(directoryPath)
    {
        InsertFetchButton();
    }

    private void PopulateAndFetch(string selectedTheme, bool fetch=true)
    {
        PopulateXamlFiles();
        ThemeDownloader.RefreshAvailableThemes(fetch).Wait();
        InsertFetchButton();

        if (System.IO.File.Exists(selectedTheme))
        {
            // Why do I need to do it twice? I don't know -zw
            File = selectedTheme;
            File = selectedTheme;
        }
    }

    private async Task DownloadTheme()
    {
        string selectedTheme = File!;

        var dialog = new ThemeDownloadDialog();
        var task = ThemeDownloader.DownloadThemeByName(File, dialog);
        dialog.ShowDialog();
        await task;

        bool exists = System.IO.File.Exists(selectedTheme);

        PopulateAndFetch(exists ? selectedTheme : ThemeDownloader.GetFullPath("Default.xaml"), false);

        if (exists)
        {
            // Probably the bodgey-est bodge I've ever done -zw
            File = Files.FirstOrDefault();
            File = selectedTheme;
        }
    }

    protected override void UpdateSource()
    {
        if (File == null)
            return;

        if (File == FetchText)
        {
            PopulateAndFetch(ThemeDownloader.GetFullPath(Path.GetFileName(Source.ToString())));
            return;
        }

        try
        {
            Source = new Uri(File, UriKind.RelativeOrAbsolute);
        }
        catch
        {
            DownloadTheme();
            return;
        }

        /*
            Cleanup old Dictionaries:

            Normally I wouldn't personally suggest running GC.Collect in user code however there's 
            potentially a lot of resources to clean up  in terms of memory space. Especially if e.g. 
            user loaded in complex images.

            As this in practice occurs over a theme or language switch, it should be largely unnoticeable to the end user.
        */

        OnNewFileSet();
    }

    private static int LetterIndex(string str, int letterIndex)
    {
        string letterChart = "abcdefghijklmnopqrstuvwxyz";
        return letterChart.IndexOf(str.ToLower()[letterIndex]);
    }

    // Returns true if str1 should be ordered after str2
    private static bool AlphabeticalCompare(string str1, string str2)
    {
        int length = Math.Min(str1.Length, str2.Length);
        for (int i = 0; i < length; i++)
        {
            int index1 = LetterIndex(str1, i);
            int index2 = LetterIndex(str2, i);
            if (index1 == index2) continue;

            return index1 > index2;
        }

        return str1.Length > str2.Length;
    }

    /// <summary>
    /// Places the new entry in a spot to keep the alphabetical sorting
    /// </summary>
    public void InsertAlphabetical(string item)
    {
        for (int i = Files[0] == FetchText ? 1 : 0; i < Files.Count; i++)
        {
            if (AlphabeticalCompare(Files[i], item))
            {
                Files.Insert(i, item);
                return;
            }
        }

        Files.Add(item);
    }
}
