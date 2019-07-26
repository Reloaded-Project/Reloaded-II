using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

namespace Reloaded.Mod.Loader.Update.Utilities
{
    /// <summary>
    /// Utility class around the WebClient to be used for downloading files.
    /// </summary>
    public static class FileDownloader
    {
        /// <summary>
        /// Downloads a file from the web.
        /// </summary>
        /// <param name="uri">The URI to download from.</param>
        /// <param name="downloadCompleted">The event to fire when the download completes.</param>
        /// <param name="downloadProgressChanged">The event to fire when the download progress changes.</param>
        public static async Task<byte[]> DownloadFile(Uri uri,
            DownloadDataCompletedEventHandler downloadCompleted = null,
            DownloadProgressChangedEventHandler downloadProgressChanged = null)
        {
            // Start the modification download.
            using (WebClient client = new WebClient())
            {
                client.DownloadDataCompleted += downloadCompleted;
                client.DownloadProgressChanged += downloadProgressChanged;
                return await client.DownloadDataTaskAsync(uri);
            }
        }

    }
}
