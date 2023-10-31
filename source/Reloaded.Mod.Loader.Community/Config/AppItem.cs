namespace Reloaded.Mod.Loader.Community.Config;

/// <summary>
/// Describes an individual resolved application.
/// </summary>
public class AppItem
{
    /// <summary>
    /// [XXH64] Hash of this individual item.
    /// </summary>
    public string? Hash { get; set; }

    /// <summary>
    /// Description for when game matches by ID but not by hash.
    /// </summary>
    public string? BadHashDescription { get; set; }

    /// <summary>
    /// ID of the application.
    /// </summary>
    public string? AppId { get; set; }

    /// <summary>
    /// Status of the application.
    /// </summary>
    public Status AppStatus { get; set; }

    /// <summary>
    /// Compatibility note or description.
    /// </summary>
    public string? BadStatusDescription { get; set; }

    /// <summary>
    /// Name of the application in question.
    /// </summary>
    public string? AppName { get; set; }

    /// <summary>
    /// Command line arguments when running from launcher.
    /// </summary>
    public string? AppArguments { get; set; }

    /// <summary>
    /// Game Id for GameBanana. Accept if non-zero.
    /// </summary>
    public long GameBananaId { get; set; }

    /// <summary>
    /// List of possible warnings for the application.
    /// </summary>
    public List<WarningItem> Warnings { get; set; } = new ();

    /// <summary>
    /// Verifies this game configuration for potential errors, 
    /// </summary>
    /// <param name="folder">The folder to validate game data in.</param>
    /// <param name="warnings">Error associated with the application.</param>
    public bool TryGetError(string folder, out List<WarningItem> warnings)
    {
        warnings = new List<WarningItem>();
        foreach (var warning in Warnings)
        {
            if (warning.Verify(folder))
                warnings.Add(warning);
        }

        return warnings.Count > 0;
    }
}

/// <summary>
/// Status of the game item in question.
/// </summary>
public enum Status
{
    /// <summary>
    /// Everything is ok.
    /// </summary>
    Ok,

    /// <summary>
    /// Wrong executable, such a game launcher.
    /// </summary>
    WrongExecutable
}

/// <summary>
/// An individual item describing a warning to be given.
/// </summary>
public class WarningItem
{
    /// <summary>
    /// The error message to display on successful match.
    /// </summary>
    public string? ErrorMessage { get; set; } = "";

    /// <summary>
    /// The list of items to be verified.
    /// </summary>
    public List<VerifyItem> Items { get; set; } = new ();

    /// <summary>
    /// Verifies whether this warning should be given for a given folder path.
    /// </summary>
    /// <param name="folder">Path to the folder in question.</param>
    public bool Verify(string folder)
    {
        foreach (var item in Items)
        {
            var filePath = Path.Combine(folder, item.FilePath);
            if (File.Exists(filePath))
            {
                if (string.IsNullOrEmpty(item.Hash))
                    return true;

                using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                bool result = item.Hash == Hashing.ToString(xxHash64.ComputeHash(stream));
                if (result)
                    return true;
            }
        }

        return false;
    }
}

/// <summary>
/// Encapsulates an individual item to be modified.
/// </summary>
public class VerifyItem
{
    /// <summary>
    /// [XXH64] Hash of the file. Optional.
    /// </summary>
    public string? Hash { get; set; }

    /// <summary>
    /// Relative path to the file.
    /// </summary>
    public string FilePath { get; set; } = "";
}