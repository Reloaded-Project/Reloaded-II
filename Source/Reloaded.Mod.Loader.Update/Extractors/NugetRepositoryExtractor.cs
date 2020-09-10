using System;
using System.Threading;
using System.Threading.Tasks;
using Onova.Services;
using Reloaded.Mod.Loader.Update.Utilities.Nuget;

namespace Reloaded.Mod.Loader.Update.Extractors
{
    public class NugetRepositoryExtractor : IPackageExtractor
    {
        public async Task ExtractPackageAsync(string sourceFilePath, string destDirPath, IProgress<double> progress = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            await Task.Run(() => Nuget.ExtractPackage(sourceFilePath, destDirPath, cancellationToken), cancellationToken);
        }
    }
}
