// See https://aka.ms/new-console-template for more information

using Reloaded.Mod.Loader.IO;
using Reloaded.Mod.Loader.IO.Config;
using Reloaded.Mod.Loader.IO.Structs;
using Reloaded.Mod.Loader.Update.Index.Provider;
using Reloaded.Mod.Loader.Update.Interfaces;
using Reloaded.Mod.Loader.Update.Packaging;
using Reloaded.Mod.Loader.Update.Providers.GameBanana;
using Reloaded.Mod.Loader.Update.Providers.GameBanana.Structures;
using SevenZip;
using Sewer56.DeltaPatchGenerator.Lib.Utility;
using Sewer56.Update.Misc;

var gbId = 6061;
var gbResolver = new GameBananaPackageProvider(gbId);
var items = await gbResolver.SearchAllAsync("");
var validItems = items.Where(x => string.IsNullOrEmpty(x.Id)).ToArray();

var tempFolder = new TemporaryFolderAllocation();
Console.WriteLine(tempFolder.FolderPath);
foreach (var item in validItems)
{
    try
    {
        var size = item.FileSize.GetValueOrDefault(0) / 1000.0 / 1000.0;
        Console.Write($"Item: {item.Name}, {size:0.00}MB | ");
        var downloadedFolder = await item.DownloadAsync(tempFolder.FolderPath, new Progress<double>());
        await ProcessExtractedModAsync(downloadedFolder, item);
        Console.WriteLine($"Win");
    }
    catch (Exception e)
    {
        Console.WriteLine($"Fail, {e.Message}");
    }
}

var a = 5;

async Task ProcessExtractedModAsync(string folderPath, IDownloadablePackage downloadablePackage)
{
    if (string.IsNullOrEmpty(folderPath))
    {
        Console.WriteLine($"Problem: {downloadablePackage.Name}");
        return;
    }

    var path = Path.Combine(folderPath, ModConfig.ConfigFileName);
    var config = ConfigReader<ModConfig>.ReadConfiguration(path);
    var data = (GameBananaMod)downloadablePackage.ExtraData;
    var gbUpdateData = new GameBananaUpdateResolverFactory.GameBananaConfig()
    {
        ItemId = data.Id
    };

    var tuple = new PathTuple<ModConfig>(path, config);
    Singleton<GameBananaUpdateResolverFactory>.Instance.SetConfiguration(tuple, gbUpdateData);
    ConfigReader<ModConfig>.WriteConfiguration(path, config);

    var publisher = await Publisher.PublishAsync(new Publisher.PublishArgs()
    {
        PublishTarget = Publisher.PublishTarget.GameBanana,
        OutputFolder = Path.Combine(folderPath, "..", "Publish", $"{config.ModId}"),
        CompressionLevel = CompressionLevel.Ultra,
        CompressionMethod = CompressionMethod.Lzma2,
        ModTuple = tuple,
        MetadataFileName = tuple.Config.ReleaseMetadataFileName
    });
}