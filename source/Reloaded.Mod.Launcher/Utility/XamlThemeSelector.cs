namespace Reloaded.Mod.Launcher.Utility;

public class XamlThemeSelector : XamlFileSelector
{
    public XamlThemeSelector(string directoryPath) : base(directoryPath)
    {
        Files.Insert(0, "Fetch Themes...");
    }

    private void PopulateAndFetch(bool fetch=true)
    {
        PopulateXamlFiles();

        ThemeDownloader.RefreshAvailableThemes(fetch).Wait();

        Files.Insert(0, "Fetch Themes...");
    }

    protected override void UpdateSource()
    {
        if (File == null)
            return;

        if (File == "Fetch Themes...")
        {
            PopulateAndFetch();
            return;
        }

        try
        {
            Source = new Uri(File, UriKind.RelativeOrAbsolute);
        }
        catch
        {
            DownloadPackageViewModel viewModel = new([], Lib.IoC.Get<LoaderConfig>())
            {
                Progress = 0,
                Text = "Downloading Theme",
                Packages = [],
                DownloadTask = ThemeDownloader.DownloadThemeByName(File)
            };
            var dialog = new DownloadPackageDialog(viewModel);
            dialog.ShowDialog();
            dialog.ViewModel.DownloadTask!.Wait();

            PopulateAndFetch(false);
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
}
