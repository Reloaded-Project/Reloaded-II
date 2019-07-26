using System;
using System.Collections.Generic;
using System.Text;

namespace Reloaded.Mod.Loader.Update.Structures
{
    public class ModUpdate
    {
        public string   ModId { get; set; }
        public Version  OldVersion { get; set; }
        public Version  NewVersion { get; set; }
        public long     UpdateSize { get; set; }

        // ReSharper disable once InconsistentNaming
        public float   UpdateSizeMB => UpdateSize / 1000F / 1000F;

        public ModUpdate(string modId, Version oldVersion, Version newVersion, long updateSize)
        {
            ModId = modId;
            OldVersion = oldVersion;
            NewVersion = newVersion;
            UpdateSize = updateSize;
        }
    }
}
