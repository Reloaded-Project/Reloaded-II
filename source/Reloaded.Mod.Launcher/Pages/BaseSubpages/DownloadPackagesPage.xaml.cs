namespace Reloaded.Mod.Launcher.Pages.BaseSubpages;

/// <summary>
/// Interaction logic for DownloadModsPage.xaml
/// </summary>
public partial class DownloadPackagesPage : ReloadedIIPage
{
    public DownloadPackagesViewModel ViewModel { get; set; }

    public DownloadPackagesPage()
    {
        InitializeComponent();

        ViewModel = Lib.IoC.Get<DownloadPackagesViewModel>();
        this.AnimateInStarted += TryGetSearchResults;
    }

    private async void TryGetSearchResults()
    {
        try
        {
            await ViewModel.GetSearchResults();
        }
        catch (Exception ex)
        {
            Errors.HandleException(ex, "Failed to search for mods to download.");
        }
    }

    private async void Last_Click(object sender, RoutedEventArgs e) => await ViewModel.GoToLastPage();
    private async void Next_Click(object sender, RoutedEventArgs e) => await ViewModel.GoToNextPage();
}