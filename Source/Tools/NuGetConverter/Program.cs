using Reloaded.Mod.Loader.Update.Converters.NuGet;
using System;
using System.IO;

namespace NuGetConverter
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Reloaded-II NuGet Package Converter\n" +
                              "Converts mod folders or archives into NuGet packages\n" +
                              "Usage: NuGetConverter.exe <Mod Folder or Archive Path> <Output Path>\n" +
                              "Example: NuGetConverter.exe Mod.zip .\n" +
                              "Example: NuGetConverter.exe reloaded.test.mod .\n" +
                              "Example: NuGetConverter.exe reloaded.text.mod ./packages/");

            var input  = args[0];
            var output = args[1];
            if (File.GetAttributes(input).HasFlag(FileAttributes.Directory))
                Move(Converter.FromModDirectory(input, output), output);
            else
                Move(Converter.FromArchiveFile(input, output), output);
        }

        static void Move(string filePath, string outputFolder)
        {
            var newPath = Path.Combine(outputFolder, Path.GetFileName(filePath));
            Console.WriteLine($"Writing to: {newPath}");
            File.Move(filePath, newPath, true);
        }
    }
}
