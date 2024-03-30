using System.Text;
using Reloaded.Mod.Loader.Update.Providers.Web;
using Sewer56.DeltaPatchGenerator.Lib.Utility;
using Sewer56.Update.Extractors.SevenZipSharp;
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
    }

    protected override AutomationPeer OnCreateAutomationPeer() => new EmptyAutomationPeer(this);

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
            await archiveExtractor.ExtractPackageAsync(file, tempFolder.FolderPath, new Progress<double>(), default);

            /* Get name of package. */
            WebDownloadablePackage.CopyPackagesFromExtractFolderToTargetDir(modsFolder!, tempFolder.FolderPath, default);
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