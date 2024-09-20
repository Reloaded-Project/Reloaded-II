namespace Reloaded.Mod.Installer.Lib.Utilities
{
    // ReSharper disable once InconsistentNaming
    public static class IOEx
    {
        /// <summary>
        /// Tries to delete a directory, if possible.
        /// </summary>
        public static void TryDeleteDirectory(string path, bool recursive = true)
        {
            try { Directory.Delete(path, recursive); }
            catch (Exception) { /* Ignored */ }
        }
    }
}