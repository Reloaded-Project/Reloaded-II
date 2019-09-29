using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Onova.Services;
using Reloaded.Mod.Loader.Update.Utilities;

namespace Reloaded.Mod.Loader.Update.Extractors
{
    public class NugetRepositoryExtractor : IPackageExtractor
    {
        public async Task ExtractPackageAsync(string sourceFilePath, string destDirPath, IProgress<double> progress = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            await Task.Run(() => NugetHelper.ExtractPackageContent(sourceFilePath, destDirPath, cancellationToken), cancellationToken);
        }
    }
}
