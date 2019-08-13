using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using Reloaded.Mod.Loader.Update.Converters.NuGet;
using Reloaded.Mod.Loader.Update.Exceptions;
using Xunit;

namespace Reloaded.Mod.Loader.Tests.Update.NuGet.Convert
{
    public class ConverterTests
    {
        private static string TestArchiveFile = "HeroesControllerPostProcess.zip";
        private static string TestArchiveFileBad = "HeroesControllerPostProcessBad.zip"; // Has folder in root.

        [Fact]
        public void TryConvertBad()
        {
            var converter = new Converter();
            Assert.Throws<BadArchiveException>(() => converter.ConvertFromArchiveFile(TestArchiveFileBad, Environment.CurrentDirectory));
        }

        [Fact]
        public void TryConvert()
        {
            var converter = new Converter();
            var converted = converter.ConvertFromArchiveFile(TestArchiveFile, Environment.CurrentDirectory);

            Assert.True(File.Exists(converted));
            Assert.True(IsZipValid(converted));
        }

        private static bool IsZipValid(string path)
        {
            try
            {
                using (var zipFile = ZipFile.OpenRead(path))
                {
                    var entries = zipFile.Entries;
                    return true;
                }
            }
            catch (InvalidDataException)
            {
                return false;
            }
        }

    }
}
