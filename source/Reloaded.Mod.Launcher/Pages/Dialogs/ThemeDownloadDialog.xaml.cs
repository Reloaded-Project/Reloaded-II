using Reloaded.Mod.Loader.Update.Providers.GameBanana.Structures;
using System.IO.Compression;
using System.Net.Http;
using System.Net.Http.Handlers;

namespace Reloaded.Mod.Launcher.Pages.Dialogs;

/// <summary>
/// Interaction logic for ThemeDownloadDialog.xaml
/// </summary>
public partial class ThemeDownloadDialog : ReloadedWindow
{
    private readonly CancellationTokenSource CancellationToken;
    private readonly Task DownloadTask;

    public ThemeDownloadDialog(GameBananaModFile file)
    {
        InitializeComponent();

        CancellationToken = new CancellationTokenSource();

        DownloadTask = DownloadAndExtractZip(file);
        ShowDialog();
        DownloadTask.Wait();
    }

    public async Task DownloadAndExtractZip(GameBananaModFile file)
    {
        ThemeTextBlock.Text = $"Downloading {file.FileName}...";

        var handler = new HttpClientHandler() { AllowAutoRedirect = true };
        var progressHandler = new ProgressMessageHandler(handler);

        // This is supposed to update the progress bar as it's downloading, emphasis on supposed to
        progressHandler.HttpReceiveProgress += (obj, args) => { ThemeProgressBar.Value = (args.BytesTransferred / (double)args.TotalBytes!) * 100.0; };

        var zipStream = await new HttpClient(progressHandler).GetStreamAsync(file.DownloadUrl);

        ThemeTextBlock.Text = $"Extracting {file.FileName}...";
        ZipFile.ExtractToDirectory(zipStream, ThemeDownloader.TempFolder);

        zipStream.Close();

        Close();
    }

    private void CancelButton_PreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (CancellationToken.Token.CanBeCanceled)
            CancellationToken.Cancel();

        Close();
    }
}
