using System.Text.Json.Nodes;

namespace Reloaded.Mod.Launcher.Lib.Models.Model.Configuration.Native.Schema;

/// <summary>
/// Parameters for the folder picker control; native equivalent of
/// <see cref="Reloaded.Mod.Interfaces.Structs.FolderPickerParamsAttribute"/>.
/// </summary>
public class FolderPicker
{
    /// <summary>
    /// Initial directory shown; null for the default.
    /// </summary>
    public string? InitialDirectory { get; set; }

    /// <summary>
    /// Fallback folder when <see cref="InitialDirectory"/> is null, as an
    /// <c>Environment.SpecialFolder</c> value.
    /// </summary>
    public int InitialFolderPath { get; set; } = 0x05; // Environment.SpecialFolder.Personal

    /// <summary>
    /// Label of the choose folder button.
    /// </summary>
    public string ChooseFolderButtonLabel { get; set; } = "Choose Folder";

    /// <summary>
    /// Allow typing in the path box.
    /// </summary>
    public bool UserCanEditPathText { get; set; } = true;

    /// <summary>
    /// Title of the dialog.
    /// </summary>
    public string Title { get; set; } = "";

    /// <summary>
    /// Label of the OK button.
    /// </summary>
    public string OkButtonLabel { get; set; } = "Ok";

    /// <summary>
    /// Label of the file name box.
    /// </summary>
    public string FileNameLabel { get; set; } = "";

    /// <summary>
    /// Allow selecting multiple folders.
    /// </summary>
    public bool Multiselect { get; set; } = false;

    /// <summary>
    /// Only accept folders in the file system.
    /// </summary>
    public bool ForceFileSystem { get; set; } = false;

    /// <summary>
    /// Reads the folder picker parameters from their JSON representation.
    /// </summary>
    /// <param name="node">Node holding the picker's properties.</param>
    public static FolderPicker Parse(JsonNode node) => new()
    {
        InitialDirectory       = node.GetStringOrDefault(Keys.InitialDirectory, null),
        InitialFolderPath      = node.GetIntOrDefault(Keys.InitialFolderPath, 0x05),
        ChooseFolderButtonLabel = node.GetStringOrDefault(Keys.ChooseFolderButtonLabel, "Choose Folder")!,
        UserCanEditPathText    = node.GetBoolOrDefault(Keys.UserCanEditPathText, true),
        Title                  = node.GetStringOrDefault(Keys.Title, "")!,
        OkButtonLabel          = node.GetStringOrDefault(Keys.OkButtonLabel, "Ok")!,
        FileNameLabel          = node.GetStringOrDefault(Keys.FileNameLabel, "")!,
        Multiselect            = node.GetBoolOrDefault(Keys.Multiselect, false),
        ForceFileSystem        = node.GetBoolOrDefault(Keys.ForceFileSystem, false)
    };
}
