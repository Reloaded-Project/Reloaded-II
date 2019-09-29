using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Reloaded.Mod.Loader.IO;
using Reloaded.Mod.Loader.IO.Config;
using Reloaded.Mod.Loader.Update.Extractors;
using Reloaded.Mod.Loader.Update.Utilities;
using Reloaded.WPF.MVVM;
using Reloaded.WPF.Theme.Default;

namespace Reloaded.Mod.Launcher.Pages.Dialogs
{
    /// <summary>
    /// Interaction logic for DownloadModArchive.xaml
    /// </summary>
    public partial class DownloadModArchiveDialog : ReloadedWindow
    {
        public new DownloadModArchiveViewModel ViewModel { get; set; }

        public DownloadModArchiveDialog(IEnumerable<Uri> uri)
        {
            InitializeComponent();
            ViewModel = new DownloadModArchiveViewModel(uri);
        }

        private async void Download_Click(object sender, RoutedEventArgs e)
        {
            var senderButton = (Button) sender;
            senderButton.IsEnabled = false; // Breaking MVVM, don't care in this case.
            await ViewModel.DownloadAndExtractAll();
            this.Close();
        }
    }

    public class DownloadModArchiveViewModel : ObservableObject
    {
        public ObservableCollection<DownloadModDetails> Mods { get; set; } = new ObservableCollection<DownloadModDetails>();
        public int Progress { get; set; }

        /* Setup & Teardown */
        public DownloadModArchiveViewModel(IEnumerable<Uri> uri)
        {
            foreach (var modUri in uri)
            {
                Mods.Add(new DownloadModDetails(modUri));
            }
        }

        /* Public Members */
        public async Task DownloadAndExtractAll()
        {
            long modsComplete   = 0;
            long totalMods      = Mods.Count;
            float maxProgressPerMod = 1 / (float) totalMods;

            foreach (var mod in Mods)
            {
                await mod.DownloadAndExtract((sender, args) =>
                {
                    var currentModProgress = args.BytesReceived / (float)args.TotalBytesToReceive;
                    var scaledModProgress  = currentModProgress / totalMods;
                    var progressFromOtherMods = maxProgressPerMod * modsComplete;
                    Progress = (int)Math.Round(scaledModProgress + progressFromOtherMods);
                });

                modsComplete += 1;
            }
        }
    }

    public class DownloadModDetails : ObservableObject
    {
        public Uri Uri          { get; set; }
        public string FileName  { get; set; }
        public float  FileSize  { get; set; }
        public int Progress     { get; set; }

        public DownloadModDetails(Uri uri)
        {
            Uri = uri;

            #pragma warning disable 4014
            GetNameAndSize(uri); // Fire and forget.
            #pragma warning restore 4014
        }

        private async Task GetNameAndSize(Uri url)
        {
            using (WebClient client = new WebClient())
            {
                // Obtain the name of the file.
                try
                {
                    await client.OpenReadTaskAsync(url);

                    try { FileName = GetFileName(client); }
                    catch (Exception) { FileName = "???"; }

                    try { FileSize = GetFileSize(client); }
                    catch (Exception) { FileSize = -1; }
                }
                catch (Exception) { /* Probably shouldn't swallow this one, but will for now. */ }
            }
        }

        private string GetFileName(WebClient client) => new ContentDisposition(client.ResponseHeaders["Content-Disposition"]).FileName;
        private float GetFileSize(WebClient client)  => Convert.ToInt64(client.ResponseHeaders["Content-Length"]) / 1000F / 1000F;

        /// <summary>
        /// Downloads a specific file.
        /// </summary>
        public async Task DownloadAndExtract(DownloadProgressChangedEventHandler progressChanged)
        {
            // Start the modification download.
            byte[] data;
            using (WebClient client = new WebClient())
            {
                client.DownloadProgressChanged += progressChanged;
                data = await client.DownloadDataTaskAsync(Uri);
            }

            /* Extract to Temp Directory */
            string temporaryDirectory = GetTemporaryDirectory();
            var archiveExtractor = new ArchiveExtractor();
            await archiveExtractor.ExtractPackageAsync(data, temporaryDirectory);

            /* Get name of package. */
            var configReader = new ConfigReader<ModConfig>();
            var configs = configReader.ReadConfigurations(temporaryDirectory, ModConfig.ConfigFileName);
            var loaderConfig = LoaderConfigReader.ReadConfiguration();

            foreach (var config in configs)
            {
                string configId = config.Object.ModId;
                string configDirectory = Path.GetDirectoryName(config.Path);
                string targetDirectory = Path.Combine(loaderConfig.ModConfigDirectory, configId);
                IOEx.MoveDirectory(configDirectory, targetDirectory);
            }

            Directory.Delete(temporaryDirectory, true);
        }

        private string GetTemporaryDirectory()
        {
            string tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tempDirectory);
            return tempDirectory;
        }
    }
}
