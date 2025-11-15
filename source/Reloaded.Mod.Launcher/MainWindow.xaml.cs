using NuGet.Common;
using Reloaded.Mod.Loader.Update.Providers.Web;
using Sewer56.DeltaPatchGenerator.Lib.Utility;
using Sewer56.Update.Extractors.SevenZipSharp;
using System.Text;
using System.Windows.Threading;
using static Reloaded.Mod.Launcher.Lib.Static.Resources;

namespace Reloaded.Mod.Launcher;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : ReloadedWindow
{
    private string[] SupportedDropFormats = [".7z", ".zip", ".rar"];
    
    public Lib.Models.ViewModel.WindowViewModel RealViewModel { get; set; }

    public MainWindow()
    {
        // Make viewmodel of this window available.
        RealViewModel = Lib.IoC.GetConstant<Lib.Models.ViewModel.WindowViewModel>();

        // Initialize XAML.
        InitializeComponent();

        // Bind other models.
        Lib.IoC.BindToConstant((WPF.Theme.Default.WindowViewModel)DataContext);// Controls window properties.
        Lib.IoC.BindToConstant(this);

#if DEBUG
        // Hide during dev so dev tools aren't blocked.
        if (Debugger.IsAttached)
            BorderDragDropCapturer.Visibility = Visibility.Collapsed;
#endif

        // Allow DragDrop across entire app by using a transparent Border to
        // capture mouse events first. MouseEnter will fire after DragDrop is finished.
        this.MouseEnter += (sender, args) => { this.BorderDragDropCapturer.IsHitTestVisible = false; };
        this.MouseLeave += (sender, args) => { this.BorderDragDropCapturer.IsHitTestVisible = true; };
    }

    private void InstallMod_DragOver(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop)!;
            
            // Check if the file is a .zip file
            foreach (var file in files)
            {
                var extension = Path.GetExtension(file);
                if (!SupportedDropFormats.Contains(extension)) 
                    continue;

                e.Effects = DragDropEffects.Copy;
                e.Handled = true;
                return;
            }
        }
        else
        {
            this.BorderDragDropCapturer.IsHitTestVisible = false;
        }

        e.Handled = true;
    }

    private async void InstallMod_Drop(object sender, DragEventArgs e)
    {
        if (!e.Data.GetDataPresent(DataFormats.FileDrop)) 
            return;

        var files = (string[])e.Data.GetData(DataFormats.FileDrop)!;
        var config = Lib.IoC.GetConstant<LoaderConfig>();
        var modsFolder = config.GetModConfigDirectory();

        // Get list of installed mods before
        var modConfigService = Lib.IoC.GetConstant<ModConfigService>();
        var modsBefore = new Dictionary<string, PathTuple<ModConfig>>(modConfigService.ItemsById);

        // Install mods.
        foreach (var file in files)
        {
            var extension = Path.GetExtension(file);
            if (!SupportedDropFormats.Contains(extension)) 
                continue;

            /* Extract to Temp Directory */
            using var tempFolder = new TemporaryFolderAllocation();
            var archiveExtractor = new SevenZipSharpExtractor();

            var installVM = new InstallPackageViewModel
            {
                Title = InstallModArchiveTitle.Get(),
                Text = ExtractingLocalModArchive.Get(),
                Progress = 0
            };

            var progress = new Progress<double>(value =>
            {
                installVM.Progress = value * 100;
            });

            //Waits for 3 seconds before showing the install dialog, if the installation is not complete by then.
            var timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(3)
            };

            timer.Tick += (s, e) =>
            {
                timer.Stop();
                if (installVM.Progress != 100)
                {
                    Actions.ShowInstallPackageDialog.Invoke(installVM);
                }
            };
            timer.Start();

            await archiveExtractor.ExtractPackageAsync(file, tempFolder.FolderPath, progress, default);

            installVM.Text = InstallingModWait.Get();
            await WebDownloadablePackage.CopyPackagesFromExtractFolderToTargetDir(modsFolder!, tempFolder.FolderPath, default);
            installVM.IsComplete = true;
        }
        
        // Find the new mods
        modConfigService.ForceRefresh();
        var newConfigs = new List<ModConfig>();
        foreach (var item in modConfigService.ItemsById.ToArray())
        {
            if (!modsBefore.ContainsKey(item.Key))
                newConfigs.Add(item.Value.Config);
        }

        if (newConfigs.Count <= 0)
            return;
        
        // Print loaded mods.
        var installedMods = new StringBuilder();
        foreach (var conf in newConfigs)
            installedMods.AppendLine($"{conf.ModName} ({conf.ModId})");
        
        var loadedMods = string.Format(DragDropInstalledModsDescription.Get(), newConfigs.Count);
        Actions.DisplayMessagebox?.Invoke(DragDropInstalledModsTitle.Get(), 
            $"{loadedMods}\n\n{installedMods}",
            new Actions.DisplayMessageBoxParams() { StartupLocation = Actions.WindowStartupLocation.CenterScreen });
    }
}