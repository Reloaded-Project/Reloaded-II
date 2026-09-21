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
    /// <exception cref="JsonException">
    /// Thrown when the name is not a valid identifier.
    /// </exception>
    public static EnumMember Parse(JsonNode node)
    {
        var member = new EnumMember
        {
            Name        = node.GetStringOrDefault(Keys.Name, "")!,
            DisplayName = node.GetStringOrDefault(Keys.DisplayName, null)
        };

        Property.ValidateName(member.Name, $"'{Keys.Name}' of enum value '{member.Name}'");
        return member;
    }
}
