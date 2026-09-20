using System.Text.Json.Nodes;

namespace Reloaded.Mod.Launcher.Lib.Models.Model.Configuration.Native.Schema;

/// <summary>
/// Individual configuration of a native mod; native equivalent of one
/// <see cref="Reloaded.Mod.Interfaces.IConfigurable"/>.
/// </summary>
public class Configuration
{
    /// <summary>
    /// Name of the config file where the values for this configuration are stored.
    /// Defaults to <c>Config.json</c>.
    /// </summary>
    public string FileName { get; set; } = "Config.json";

    /// <summary>
    /// Name shown in the launcher's configuration dropdown.
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// Enumerations available to the properties of this configuration.
    /// </summary>
    public List<Enum> Enums { get; set; } = new();

    /// <summary>
    /// The individual settings.
    /// </summary>
    public List<Property> Properties { get; set; } = new();

    /// <summary>
    /// Reads a configuration from its JSON representation.
    /// </summary>
    /// <param name="node">Node holding the configuration's properties.</param>
    /// <exception cref="JsonException">
    /// Thrown when <c>FileName</c> is not a plain file name.
    /// </exception>
    public static Configuration Parse(JsonNode node)
    {
        var configuration = new Configuration
        {
            FileName    = ValidateFileName(node.GetStringOrDefault(Keys.FileName, "Config.json")!),
            DisplayName = node.GetStringOrDefault(Keys.DisplayName, null)
        };

        if (node[Keys.Enums] is JsonArray enums)
        {
            foreach (var enumNode in enums)
                configuration.Enums.Add(Enum.Parse(enumNode!));
        }

        if (node[Keys.Properties] is JsonArray properties)
        {
            foreach (var propertyNode in properties)
                configuration.Properties.Add(Property.Parse(propertyNode!));
        }

        return configuration;
    }

    /// <summary>
    /// The file name builds paths inside the user config directory, so it
    /// must be a plain file name; rooted paths and separators fail validation.
    /// </summary>
    private static string ValidateFileName(string fileName)
    {
        if (fileName.Length <= 0 || Path.IsPathRooted(fileName) || fileName != Path.GetFileName(fileName))
            throw new JsonException($"'{Keys.FileName}' must be a plain file name, got '{fileName}'.");

        return fileName;
    }
}
