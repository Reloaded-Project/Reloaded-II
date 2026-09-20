using System.Text.Json.Nodes;

namespace Reloaded.Mod.Launcher.Lib.Models.Model.Configuration.Native;

/// <summary>
/// Declarative configuration schema for native (non .NET) mods.
/// 
/// A mod declares its settings in a <c>ConfigSchema.json</c> file next to
/// its <c>ModConfig.json</c>. The launcher builds the configuration UI
/// from that schema.
///
/// The schema is the native equivalent of the attributes C# mods declare,
/// such as <see cref="Reloaded.Mod.Interfaces.Structs.SliderControlParamsAttribute"/>.
/// Native and C# mods therefore look and behave the same:
/// - <see cref="Schema.Property.DisplayName"/>, <see cref="Schema.Property.Description"/>,
///   <see cref="Schema.Property.Category"/> and <see cref="Schema.Property.DefaultValue"/>
/// - <see cref="Schema.Slider"/>, <see cref="Schema.FilePicker"/> and
///   <see cref="Schema.FolderPicker"/> control params
///
/// The individual schema models live in the <see cref="Schema"/> namespace.
/// </summary>
public class ModConfigSchema
{
    /// <summary>
    /// Name of the config file to place inside the mod folder.
    /// </summary>
    public const string SchemaFileName = "ConfigSchema.json";

    /// <summary>
    /// The individual configurations (pages/files) exposed by the mod.
    /// </summary>
    public List<Schema.Configuration> Configurations { get; set; } = new();

    /// <summary>
    /// Returns true if the specified mod directory contains a config schema.
    /// </summary>
    /// <param name="modDirectory">Full path to the folder containing the mod.</param>
    public static bool ExistsInFolder(string modDirectory) => File.Exists(Path.Combine(modDirectory, SchemaFileName));

    /// <summary>
    /// Loads config schema from local disk.
    /// </summary>
    /// <param name="modDirectory">Full path to the folder containing the mod.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the schema is empty or invalid. A missing file throws
    /// <see cref="FileNotFoundException"/>; malformed JSON,
    /// <see cref="JsonException"/>.
    /// </exception>
    public static ModConfigSchema Load(string modDirectory) => Parse(JsonNode.Parse(File.ReadAllText(Path.Combine(modDirectory, SchemaFileName)), new JsonNodeOptions() { PropertyNameCaseInsensitive = true }) ?? throw newException(modDirectory), modDirectory);

    private static Exception newException(string modDirectory) => new InvalidOperationException($"Failed to parse {SchemaFileName} in '{modDirectory}'. The file may be empty or invalid.");

    private static ModConfigSchema Parse(JsonNode node, string modDirectory)
    {
        try
        {
            var schema = new ModConfigSchema();
            if (node[Schema.Keys.Configurations] is JsonArray configurations)
            {
                foreach (var configurationNode in configurations)
                    schema.Configurations.Add(Schema.Configuration.Parse(configurationNode!));
            }

            if (schema.Configurations.Count <= 0)
                throw new JsonException($"Schema requires at least one entry in '{Schema.Keys.Configurations}'.");

            return schema;
        }
        catch (Exception e) when (e is JsonException or InvalidOperationException or FormatException)
        {
            throw new InvalidOperationException($"Failed to parse {SchemaFileName} in '{modDirectory}'. Check the inner exception for details.", e);
        }
    }
}
