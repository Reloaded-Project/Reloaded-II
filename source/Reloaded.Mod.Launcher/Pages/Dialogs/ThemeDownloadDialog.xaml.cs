using Reloaded.Mod.Loader.Update.Providers.GameBanana.Structures;
using System.IO.Compression;
using System.Net.Http;
using System.Net.Http.Handlers;

namespace Reloaded.Mod.Launcher.Pages.Dialogs;

public enum ThemeDownloadDialogResult
{
    Ok,
    Cancelled,
    Failed
}


/// <summary>
/// Interaction logic for ThemeDownloadDialog.xaml
/// </summary>
public partial class ThemeDownloadDialog : ReloadedWindow
{
    private CancellationTokenSource CancellationToken;

    public ThemeDownloadDialog()
    {
        InitializeComponent();

        CancellationToken = new CancellationTokenSource();
    }

    // progress is measured from 0-1
    private void UpdateProgressBar(double progress)
    {
        for (int i = 0; i < 2; i++)
            ((System.Windows.Media.LinearGradientBrush)ThemeTextBlock.Foreground).GradientStops[i].Offset = progress;

        ThemeProgressBar.Value = progress * 100;
    }

    public async Task<ThemeDownloadDialogResult> DownloadAndExtractZip(GameBananaModFile file)
    {
        CancellationToken = new CancellationTokenSource();

        var result = ThemeDownloadDialogResult.Ok;

        UpdateProgressBar(0);
        ThemeTextBlock.Text = $"Downloading {file.FileName}...";

        var handler = new HttpClientHandler() { AllowAutoRedirect = true };
        var progressHandler = new ProgressMessageHandler(handler);

        progressHandler.HttpReceiveProgress += (obj, args) => { UpdateProgressBar(args.BytesTransferred / (double)args.TotalBytes!); };

        int attempts = 0;
        Stream zipStream;
    Retry:
        try
        {
            zipStream = await new HttpClient(progressHandler).GetStreamAsync(file.DownloadUrl, CancellationToken.Token);
        }
        catch
        {
            if (attempts++ < 10)
                goto Retry;

            var messageBox = new MessageBox("Theme Download Error", "Failed to download the theme! Check your internet connection, and if that's good, it might just be GameBanana's servers acting up, try again later");
            messageBox.ShowDialog();

            result = ThemeDownloadDialogResult.Failed;
            // I know, I know, gotos are bad, but in this case it saves a few lines of code -zw
            goto Exit;
        }

        if (CancellationToken.IsCancellationRequested)
            result = ThemeDownloadDialogResult.Cancelled;
        else
        {
            ThemeTextBlock.Text = $"Extracting {file.FileName}...";
            try
            {
                ZipFile.ExtractToDirectory(zipStream, ThemeDownloader.TempFolder);
            }
            catch
            {
                result = ThemeDownloadDialogResult.Failed;
                goto Exit;
            }
        }

        zipStream.Close();

    Exit:
        return result;
    }

    private void CancelButton_PreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (CancellationToken.Token.CanBeCanceled)
            CancellationToken.Cancel();

        Close();
    }
}
