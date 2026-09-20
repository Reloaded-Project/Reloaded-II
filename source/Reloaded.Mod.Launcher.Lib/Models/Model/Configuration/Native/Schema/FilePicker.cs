using System.Text.Json.Nodes;

namespace Reloaded.Mod.Launcher.Lib.Models.Model.Configuration.Native.Schema;

/// <summary>
/// Parameters for the file picker control; native equivalent of
/// <see cref="Reloaded.Mod.Interfaces.Structs.FilePickerParamsAttribute"/>.
/// </summary>
public class FilePicker
{
    /// <summary>
    /// Initial directory shown; null for the default.
    /// </summary>
    public string? InitialDirectory { get; set; }

    /// <summary>
    /// Fallback folder when <see cref="InitialDirectory"/> is null, as an
    /// <see cref="System.Environment.SpecialFolder"/> value.
    /// </summary>
    public int InitialFolderPath { get; set; } = 0x05; // Environment.SpecialFolder.Personal

    /// <summary>
    /// Label of the choose file button.
    /// </summary>
    public string ChooseFileButtonLabel { get; set; } = "Choose File";

    /// <summary>
    /// Allow typing in the path box.
    /// </summary>
    public bool UserCanEditPathText { get; set; } = true;

    /// <summary>
    /// Title of the dialog.
    /// </summary>
    public string Title { get; set; } = "";

    /// <summary>
    /// Filter of the dialog, e.g. <c>All files (*.*)|*.*</c>.
    /// </summary>
    public string Filter { get; set; } = "All files (*.*)|*.*";

    /// <summary>
    /// Index of the filter selected at open.
    /// </summary>
    public int FilterIndex { get; set; } = 0;

    /// <summary>
    /// Allow selecting multiple files.
    /// </summary>
    public bool Multiselect { get; set; } = false;

    /// <summary>
    /// Support extensions with multiple dots, e.g. <c>.tar.gz</c>.
    /// </summary>
    public bool SupportMultiDottedExtensions { get; set; } = false;

    /// <summary>
    /// Show hidden files in the dialog.
    /// </summary>
    public bool ShowHiddenFiles { get; set; } = false;

    /// <summary>
    /// Show the file preview pane.
    /// </summary>
    public bool ShowPreview { get; set; } = false;

    /// <summary>
    /// Restore the working directory after the dialog closes.
    /// </summary>
    public bool RestoreDirectory { get; set; } = false;

    /// <summary>
    /// Add the chosen file to the recent documents.
    /// </summary>
    public bool AddToRecent { get; set; } = false;

    /// <summary>
    /// Reads the file picker parameters from their JSON representation.
    /// </summary>
    /// <param name="node">Node holding the picker's properties.</param>
    public static FilePicker Parse(JsonNode node) => new()
    {
        InitialDirectory             = node.GetStringOrDefault(Keys.InitialDirectory, null),
        InitialFolderPath            = node.GetIntOrDefault(Keys.InitialFolderPath, 0x05),
        ChooseFileButtonLabel        = node.GetStringOrDefault(Keys.ChooseFileButtonLabel, "Choose File")!,
        UserCanEditPathText          = node.GetBoolOrDefault(Keys.UserCanEditPathText, true),
        Title                        = node.GetStringOrDefault(Keys.Title, "")!,
        Filter                       = node.GetStringOrDefault(Keys.Filter, "All files (*.*)|*.*")!,
        FilterIndex                  = node.GetIntOrDefault(Keys.FilterIndex, 0),
        Multiselect                  = node.GetBoolOrDefault(Keys.Multiselect, false),
        SupportMultiDottedExtensions = node.GetBoolOrDefault(Keys.SupportMultiDottedExtensions, false),
        ShowHiddenFiles              = node.GetBoolOrDefault(Keys.ShowHiddenFiles, false),
        ShowPreview                  = node.GetBoolOrDefault(Keys.ShowPreview, false),
        RestoreDirectory             = node.GetBoolOrDefault(Keys.RestoreDirectory, false),
        AddToRecent                  = node.GetBoolOrDefault(Keys.AddToRecent, false)
    };
}
