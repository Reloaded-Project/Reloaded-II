namespace NuGetConverter;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Reloaded-II NuGet Package Converter\n" +
                          "Converts mod folders or archives into NuGet packages\n" +
                          "Usage: NuGetConverter.exe <Mod Folder or Archive Path> <Output Path>\n" +
                          "Example: NuGetConverter.exe Mod.zip Mod.nupkg\n" +
                          "Example: NuGetConverter.exe reloaded.test.mod reloaded.test.mod.nupkg\n" +
                          "Example: NuGetConverter.exe reloaded.test.mod ./packages/reloaded.test.mod.nupkg");

        var input  = args[0];
        var output = args[1];
        if (File.GetAttributes(input).HasFlag(FileAttributes.Directory))
            Move(await Converter.FromModDirectoryAsync(input, Path.GetDirectoryName(output)), Path.GetFullPath(output));
        else
            Move(await Converter.FromArchiveFileAsync(input, Path.GetDirectoryName(output)), Path.GetFullPath(output));
    }

    static void Move(string filePath, string outputPath)
    {
        Console.WriteLine($"Writing to: {outputPath}");
        File.Move(filePath, outputPath, true);
    }
}