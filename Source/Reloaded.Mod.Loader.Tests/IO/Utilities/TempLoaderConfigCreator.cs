using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Reloaded.Mod.Loader.IO;
using Reloaded.Mod.Loader.IO.Config;

namespace Reloaded.Mod.Loader.Tests.IO.Utilities
{
    public class TempLoaderConfigCreator : IDisposable
    {
        private static LoaderConfigReader _configReader;
        private LoaderConfig _originalConfig;

        /* Before Test */
        public TempLoaderConfigCreator()
        {
            _configReader = new LoaderConfigReader();

            if (_configReader.ConfigurationExists())
                _originalConfig = _configReader.ReadConfiguration();
            else
                _configReader.WriteConfiguration(new LoaderConfig());
        }

        /* After Test */
        public void Dispose()
        {
            // Delete all temp loader directories.
            var currentLoaderConfig = _configReader.ReadConfiguration();
            TryCatchIgnoreCode(() => Directory.Delete(currentLoaderConfig.ApplicationConfigDirectory, true));
            TryCatchIgnoreCode(() => Directory.Delete(currentLoaderConfig.ModConfigDirectory, true));
            TryCatchIgnoreCode(() => Directory.Delete(currentLoaderConfig.PluginConfigDirectory, true));

            if (_originalConfig != null)
                _configReader.WriteConfiguration(_originalConfig);
            else
                File.Delete(_configReader.ConfigurationPath());
        }

        private void TryCatchIgnoreCode(Action code)
        {
            try
            {
                code();
            }
            catch (Exception)
            {
                /* ignored */
            }
        }
    }
}
