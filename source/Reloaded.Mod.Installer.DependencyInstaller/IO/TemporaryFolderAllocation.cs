namespace Reloaded.Mod.Installer.DependencyInstaller.IO;

/// <summary>
/// Creates a temporary folder that is disposed with the class.
/// </summary>
public class TemporaryFolderAllocation : IDisposable
{
    /// <summary>
    /// Path of the temporary folder.
    /// </summary>
    public string FolderPath { get; set; }

    /// <summary/>
    public TemporaryFolderAllocation()
    {
        FolderPath = MakeUniqueFolder(Path.GetTempPath());
    }

    /// <inheritdoc />
    ~TemporaryFolderAllocation() => Dispose();

    /// <inheritdoc />
    public void Dispose()
    {
        try { Directory.Delete(FolderPath, true); }
        catch (Exception) { }
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Makes a unique, empty folder inside a specified folder.
    /// </summary>
    /// <param name="folder">The path of the folder to make folder inside.</param>
    private static string MakeUniqueFolder(string folder)
    {
        string fullPath;

        do
        {
            fullPath = Path.Combine(folder, Path.GetRandomFileName());
        }
        while (Directory.Exists(fullPath));

        Directory.CreateDirectory(fullPath);
        return fullPath;
    }
}