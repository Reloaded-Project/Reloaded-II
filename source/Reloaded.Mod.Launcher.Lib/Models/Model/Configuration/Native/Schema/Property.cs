using System.Text.Json.Nodes;

namespace Reloaded.Mod.Launcher.Lib.Models.Model.Configuration.Native.Schema;

/// <summary>
/// An individual setting of a configuration; native equivalent of a
/// property on an <see cref="Reloaded.Mod.Interfaces.IConfigurable"/> implementation.
/// </summary>
public class Property
{
    /// <summary>
    /// Supported values for <see cref="Type"/>.
    /// </summary>
    internal static class SupportedTypes
    {
        public const string Bool   = "bool";
        public const string Int    = "int";
        public const string Float  = "float";
        public const string Double = "double";
        public const string String = "string";
    }

    /// <summary>
    /// Name of the setting, stored in the config file.
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// Type of the setting; one of the following:
    /// - <c>bool</c>, <c>int</c>, <c>float</c>, <c>double</c> or <c>string</c>
    /// - the name of an enum declared in the same configuration
    /// </summary>
    public string Type { get; set; } = SupportedTypes.String;

    /// <summary>
    /// Friendly name shown in the launcher. Falls back to <see cref="Name"/>.
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// Tooltip description shown in the launcher.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Category (group) the setting is displayed under.
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    /// Sort order of the setting, lowest first.
    /// </summary>
    public int? Order { get; set; }

    /// <summary>
    /// Default value of the setting; its shape matches <see cref="Type"/>:
    /// - a <c>bool</c>, <c>int</c>, <c>float</c> or <c>double</c> literal
    /// - a <c>string</c> or an enum member name
    /// Initial value before any user change; the Reset button restores it.
    /// </summary>
    public object? DefaultValue { get; set; }

    /// <summary>
    /// Renders this setting as a slider. Only valid for numeric types.
    /// </summary>
    public Slider? Slider { get; set; }

    /// <summary>
    /// Renders this setting (string) with a file picker dialog.
    /// </summary>
    public FilePicker? FilePicker { get; set; }

    /// <summary>
    /// Renders this setting (string) with a folder picker dialog.
    /// </summary>
    public FolderPicker? FolderPicker { get; set; }

    /// <summary>
    /// Enum values declared directly on the property, for the common case
    /// of an enum used by a single setting.
    /// </summary>
    public List<EnumMember> Values { get; set; } = new();

    /// <summary>
    /// Reads a property from its JSON representation.
    /// </summary>
    /// <param name="node">Node holding the setting's properties.</param>
    /// <exception cref="JsonException">
    /// Thrown when the property has no name.
    /// </exception>
    public static Property Parse(JsonNode node)
    {
        var property = new Property
        {
            Name          = node.GetStringOrDefault(Keys.Name, "")!,
            Type          = node.GetStringOrDefault(Keys.Type, SupportedTypes.String)!.ToLowerInvariant(),
            DisplayName   = node.GetStringOrDefault(Keys.DisplayName, null),
            Description   = node.GetStringOrDefault(Keys.Description, null),
            Category      = node.GetStringOrDefault(Keys.Category, null),
            Order         = node.GetIntOrNull(Keys.Order),
            DefaultValue  = node[Keys.DefaultValue].GetValueOrNull()
        };

        if (node[Keys.Slider] is JsonNode slider)
            property.Slider = Slider.Parse(slider);

        if (node[Keys.FilePicker] is JsonNode filePicker)
            property.FilePicker = FilePicker.Parse(filePicker);

        if (node[Keys.FolderPicker] is JsonNode folderPicker)
            property.FolderPicker = FolderPicker.Parse(folderPicker);

        if (node[Keys.Values] is JsonArray values)
        {
            foreach (var valueNode in values)
            {
                var member = valueNode!.GetValueKind() == JsonValueKind.String
                    ? new EnumMember { Name = valueNode.GetValue<string>() }
                    : EnumMember.Parse(valueNode);

                if (member.Name.Length > 0)
                    property.Values.Add(member);
            }

            if (property.Values.Count > 0)
                property.Type = property.Name; // inline enums uses the property name.
        }

        if (property.Name.Length <= 0)
            throw new JsonException($"A property in the schema has no '{Keys.Name}'.");

        return property;
    }
}
