using System.Text.Json.Nodes;

namespace Reloaded.Mod.Launcher.Lib.Models.Model.Configuration.Native.Schema;

/// <summary>
/// An individual value of a schema enum.
/// </summary>
public class EnumMember
{
    /// <summary>
    /// Name of the value, stored in the config file.
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// Name shown in the launcher UI. Falls back to <see cref="Name"/>.
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// Reads an enum member from its JSON representation.
    /// </summary>
    /// <param name="node">Node holding the member's properties.</param>
    public static EnumMember Parse(JsonNode node) => new()
    {
        Name        = node.GetStringOrDefault(Keys.Name, "")!,
        DisplayName = node.GetStringOrDefault(Keys.DisplayName, null)
    };
}
