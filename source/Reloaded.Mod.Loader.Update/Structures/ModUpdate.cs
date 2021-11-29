using System;
using NuGet.Versioning;

namespace Reloaded.Mod.Loader.Update.Structures
{
    public class ModUpdate
    {
        public string   ModId { get; set; }
        public NuGetVersion  OldVersion { get; set; }
        public NuGetVersion NewVersion { get; set; }
        public long     UpdateSize { get; set; }

        // ReSharper disable once InconsistentNaming
        public float   UpdateSizeMB => UpdateSize / 1000F / 1000F;

        public ModUpdate(string modId, NuGetVersion oldVersion, NuGetVersion newVersion, long updateSize)
        {
            ModId = modId;
            OldVersion = oldVersion;
            NewVersion = newVersion;
            UpdateSize = updateSize;
        }
    }
}
