using System.Text.Json.Nodes;

namespace Reloaded.Mod.Launcher.Lib.Models.Model.Configuration.Native.Schema;

/// <summary>
/// Enumeration with display names, rendered as a list in Reloaded.
/// </summary>
public class Enum
{
    /// <summary>
    /// Name of the enum type, referenced by property <see cref="Property.Type"/>.
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// The individual values of the enum.
    /// </summary>
    public List<EnumMember> Members { get; set; } = new();

    /// <summary>
    /// Reads an enum from its JSON representation.
    /// </summary>
    /// <param name="node">Node holding the enum's properties.</param>
    /// <exception cref="JsonException">
    /// Thrown when the enum declares no members.
    /// </exception>
    public static Enum Parse(JsonNode node)
    {
        var result = new Enum
        {
            Name = node.GetStringOrDefault(Keys.Name, "")!
        };

        if (node[Keys.Members] is JsonArray members)
        {
            foreach (var memberNode in members)
            {
                if (memberNode == null)
                    throw new JsonException($"Enum '{result.Name}' has a null entry in '{Keys.Members}'.");

                var member = EnumMember.Parse(memberNode);
                if (member.Name.Length > 0)
                    result.Members.Add(member);
            }
        }

        if (result.Members.Count <= 0)
            throw new JsonException($"Enum '{result.Name}' requires at least one entry in '{Keys.Members}'.");

        return result;
    }
}
