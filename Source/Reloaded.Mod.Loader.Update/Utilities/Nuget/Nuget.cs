using System.IO;
using System.Linq;
using System.Threading;
using NuGet.Common;
using NuGet.Packaging;
using NuGet.Protocol.Core.Types;

namespace Reloaded.Mod.Loader.Update.Utilities.Nuget
{
    /// <summary>
    /// Helper class which makes it easier to interact with the NuGet API.
    /// Note: This class has a bias that always selects the latest version of every package, dependency etc.
    /// </summary>
    public class Nuget
    {
        /// <summary>
        /// Extracts the content files of the NuGet package to a specified directory.
        /// </summary>
        /// <param name="packagePath">Full path to the NuGet package.</param>
        /// <param name="targetDirectory">The directory to extract the package content to.</param>
        /// <param name="token">A cancellation token to allow cancellation of the task.</param>
        public static void ExtractPackage(string packagePath, string targetDirectory, CancellationToken token = default)
        {
            using (var packageStream = File.OpenRead(Path.GetFullPath(packagePath)))
            {
                ExtractPackage(packageStream, targetDirectory, token);
            }
        }

        /// <summary>
        /// Extracts the content files of the NuGet package to a specified directory.
        /// </summary>
        /// <param name="downloadResourceResult">Result of the download operation.</param>
        /// <param name="targetDirectory">The directory to extract the package content to.</param>
        /// <param name="token">A cancellation token to allow cancellation of the task.</param>
        public static void ExtractPackage(DownloadResourceResult downloadResourceResult, string targetDirectory, CancellationToken token = default)
        {
            if (downloadResourceResult.Status == DownloadResourceResultStatus.Available)
                ExtractPackage(downloadResourceResult.PackageStream, targetDirectory, token);
        }

        /// <summary>
        /// Extracts the content files of the NuGet package to a specified directory.
        /// </summary>
        /// <param name="stream">Stream containing the NuGet package.</param>
        /// <param name="targetDirectory">The directory to extract the package content to.</param>
        /// <param name="token">A cancellation token to allow cancellation of the task.</param>
        public static void ExtractPackage(Stream stream, string targetDirectory, CancellationToken token = default)
        {
            PackageReaderBase packageReader = new PackageArchiveReader(stream);
            var items = packageReader.GetFiles();
            var tempDirectory = $"{Path.GetTempPath()}\\{packageReader.NuspecReader.GetId()}";

            // Remove all items ending with a front or backslash (directories)
            items = items.Where(x => !(x.EndsWith("\\") || x.EndsWith("/")));

            if (Directory.Exists(tempDirectory))
                Directory.Delete(tempDirectory, true);

            packageReader.CopyFiles(tempDirectory, items, ExtractFile, new NullLogger(), token);

            var fullTargetDirectory = Path.GetFullPath(targetDirectory);
            IOEx.MoveDirectory(tempDirectory, fullTargetDirectory);
        }

        private static string ExtractFile(string sourceFile, string targetPath, Stream fileStream)
        {
            // Create directory if doesn't exist.
            var directory = Path.GetDirectoryName(targetPath);
            if (! Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            // Decompress.
            using (var targetStream = File.OpenWrite(targetPath))
            {
                fileStream.CopyTo(targetStream);
            }

            return targetPath;
        }
    }
}
