﻿using System;
using System.IO;
using System.IO.Compression;
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
            Assert.Throws<BadArchiveException>(() => Converter.FromArchiveFile(TestArchiveFileBad, Environment.CurrentDirectory));
        }

        [Fact]
        public void TryConvert()
        {
            var converted = Converter.FromArchiveFile(TestArchiveFile, Environment.CurrentDirectory);

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
