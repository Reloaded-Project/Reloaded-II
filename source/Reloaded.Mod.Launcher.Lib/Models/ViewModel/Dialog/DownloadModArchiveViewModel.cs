using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Reloaded.Mod.Launcher.Lib.Models.Model.DownloadModDialog;
using Reloaded.Mod.Loader.IO.Utility;
using Sewer56.Update.Misc;

namespace Reloaded.Mod.Launcher.Lib.Models.ViewModel.Dialog;

/// <summary>
/// ViewModel used for downloading a number of URIs from the internet.
/// </summary>
public class DownloadModArchiveViewModel : ObservableObject
{
    /// <summary>
    /// All of the mods that should be downloaded.
    /// </summary>
    public ObservableCollection<DownloadModDetails> Mods { get; set; } = new ObservableCollection<DownloadModDetails>();

    /// <summary>
    /// The current download progress.
    /// Range 0 - 100.
    /// </summary>
    public double Progress { get; set; }

    /* Setup & Teardown */

    /// <inheritdoc />
    public DownloadModArchiveViewModel(IEnumerable<Uri> uri)
    {
        foreach (var modUri in uri)
        {
            Mods.Add(new DownloadModDetails(modUri));
        }
    }

    /* Public Members */

    /// <summary>
    /// Downloads and extracts all modifications.
    /// </summary>
    public async Task DownloadAndExtractAll()
    {
        var progressSlicer = new ProgressSlicer(new Progress<double>(d => Progress = d * 100.0));

        for (var x = 0; x < Mods.Count; x++)
        {
            var mod   = Mods[x];
            var slice = progressSlicer.Slice(1.0 / Mods.Count);

            await mod.DownloadAndExtract((sender, args) =>
            {
                // We might not always get file size.
                if (args.TotalBytesToReceive <= 1)
                    return;
                
                slice.Report(args.BytesReceived / (double)args.TotalBytesToReceive);
            });

            slice.Report(1);
        }
    }
}