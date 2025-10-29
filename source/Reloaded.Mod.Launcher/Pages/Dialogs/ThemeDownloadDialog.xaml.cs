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

        int attempts = 0;
        Stream zipStream;
    Retry:
        try
        {
            zipStream = await new HttpClient(progressHandler).GetStreamAsync(file.DownloadUrl);
        }
        catch (Exception e)
        {
            if (attempts++ < 10)
                goto Retry;

            var messageBox = new MessageBox("Theme Download Error", "Failed to download the mod! Check your internet connection, and if that's good, it might just be GameBanana's servers acting up, try again later");
            messageBox.ShowDialog();

            // I know, I know, gotos are bad, but in this case it saves a few lines of code -zw
            goto Exit;
        }

        ThemeTextBlock.Text = $"Extracting {file.FileName}...";
        ZipFile.ExtractToDirectory(zipStream, ThemeDownloader.TempFolder);

        zipStream.Close();

    Exit:
        Close();
    }

    private void CancelButton_PreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (CancellationToken.Token.CanBeCanceled)
            CancellationToken.Cancel();

        Close();
    }
}
