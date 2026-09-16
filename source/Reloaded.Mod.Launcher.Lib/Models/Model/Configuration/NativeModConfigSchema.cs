using System.Text.Json.Nodes;

namespace Reloaded.Mod.Launcher.Lib.Models.Model.Configuration;

/// <summary>
/// Declarative configuration schema for native (non .NET) mods.
/// A mod declares its settings by placing a <c>ConfigSchema.json</c> file next to its <c>ModConfig.json</c>.
/// The launcher then builds a configuration UI from that schema.
/// The config file mirrors the attributes used by the C# mod template (DisplayName, Description, Category,
/// DefaultValue, Slider/File/Folder control params) so both kinds of mod look and behave the same.
/// </summary>
public class NativeModConfigSchema
{
    /// <summary>
    /// Name of the config file, need to be placed inside the mod folder.
    /// </summary>
    public const string SchemaFileName = "ConfigSchema.json";

    /// <summary>
    /// The individual configurations (pages/files) exposed by the mod.
    /// </summary>
    public List<NativeConfigSchemaConfiguration> Configurations { get; set; } = new();

    /// <summary>
    /// Returns true if the specified mod directory contains a config schema.
    /// </summary>
    /// <param name="modDirectory">Full path to the folder containing the mod.</param>
    public static bool ExistsInFolder(string modDirectory) => File.Exists(Path.Combine(modDirectory, SchemaFileName));

    /// <summary>
    /// Loads config schema from local disk.
    /// </summary>
    /// <param name="modDirectory">Full path to the folder containing the mod.</param>
    public static NativeModConfigSchema Load(string modDirectory) => Parse(JsonNode.Parse(File.ReadAllText(Path.Combine(modDirectory, SchemaFileName)), new JsonNodeOptions() { PropertyNameCaseInsensitive = true }) ?? throw newException(modDirectory), modDirectory);

    private static Exception newException(string modDirectory) => new InvalidOperationException($"Failed to parse {SchemaFileName} in '{modDirectory}'. The file may be empty or invalid.");

    private static NativeModConfigSchema Parse(JsonNode node, string modDirectory)
    {
        try
        {
            var schema = new NativeModConfigSchema();
            if (node[Keys.Configurations] is JsonArray configurations)
            {
                foreach (var configurationNode in configurations)
                    schema.Configurations.Add(NativeConfigSchemaConfiguration.Parse(configurationNode!));
            }

            if (schema.Configurations.Count <= 0)
                throw new JsonException($"Schema requires at least one entry in '{Keys.Configurations}'.");

            return schema;
        }
        catch (Exception e) when (e is JsonException or InvalidOperationException or FormatException)
        {
            throw new InvalidOperationException($"Failed to parse {SchemaFileName} in '{modDirectory}'. Check the inner exception for details.", e);
        }
    }
}

/// <summary>
/// Individual configuration of a native mod, essentially mirrors one <c>IConfigurable</c> from C# mod.
/// </summary>
public class NativeConfigSchemaConfiguration
{
    /// <summary>
    /// Name of the config file where the values for this configuration are stored.
    /// Defaults to <c>Config.json</c>, matching the C# template.
    /// </summary>
    public string FileName { get; set; } = "Config.json";

    /// <summary>
    /// Name shown in the launcher's configuration dropdown. 
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// Enumerations available to the properties of this configuration.
    /// </summary>
    public List<NativeConfigSchemaEnum> Enums { get; set; } = new();

    /// <summary>
    /// The individual settings.
    /// </summary>
    public List<NativeConfigSchemaProperty> Properties { get; set; } = new();

    public static NativeConfigSchemaConfiguration Parse(JsonNode node)
    {
        var configuration = new NativeConfigSchemaConfiguration
        {
            FileName    = ValidateFileName(node.GetStringOrDefault(Keys.FileName, "Config.json")!),
            DisplayName = node.GetStringOrDefault(Keys.DisplayName, null)
        };

        if (node[Keys.Enums] is JsonArray enums)
        {
            foreach (var enumNode in enums)
                configuration.Enums.Add(NativeConfigSchemaEnum.Parse(enumNode!));
        }

        if (node[Keys.Properties] is JsonArray properties)
        {
            foreach (var propertyNode in properties)
                configuration.Properties.Add(NativeConfigSchemaProperty.Parse(propertyNode!));
        }

        return configuration;
    }

    /// <summary>
    /// The file name is used to build paths inside the user config directory,
    /// so anything that is not a plain file name (rooted paths, separators) is rejected.
    /// </summary>
    private static string ValidateFileName(string fileName)
    {
        if (fileName.Length <= 0 || Path.IsPathRooted(fileName) || fileName != Path.GetFileName(fileName))
            throw new JsonException($"'{Keys.FileName}' must be a plain file name, got '{fileName}'.");

        return fileName;
    }
}

/// <summary>
/// Enumeration with display names, rendered as a list in Reloaded.
/// </summary>
public class NativeConfigSchemaEnum
{
    /// <summary>
    /// Name of the enum type, referenced by property <c>Type</c>.
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// The individual values of the enum.
    /// </summary>
    public List<NativeConfigSchemaEnumMember> Members { get; set; } = new();

    public static NativeConfigSchemaEnum Parse(JsonNode node)
    {
        var result = new NativeConfigSchemaEnum
        {
            Name = node.GetStringOrDefault(Keys.Name, "")!
        };

        if (node[Keys.Members] is JsonArray members)
        {
            foreach (var memberNode in members)
            {
                var member = NativeConfigSchemaEnumMember.Parse(memberNode!);
                if (member.Name.Length > 0)
                    result.Members.Add(member);
            }
        }

        if (result.Members.Count <= 0)
            throw new JsonException($"Enum '{result.Name}' requires at least one entry in '{Keys.Members}'.");

        return result;
    }
}

/// <summary>
/// An individual value of a schema enum.
/// </summary>
public class NativeConfigSchemaEnumMember
{
    /// <summary>
    /// Name of the value, stored in the config file.
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// Name shown in the launcher UI. Falls back to <see cref="Name"/>.
    /// </summary>
    public string? DisplayName { get; set; }

    public static NativeConfigSchemaEnumMember Parse(JsonNode node) => new()
    {
        Name        = node.GetStringOrDefault(Keys.Name, "")!,
        DisplayName = node.GetStringOrDefault(Keys.DisplayName, null)
    };
}

/// <summary>
/// An individual setting of a configuration; mirrors a property of a C# mod's config class.
/// </summary>
public class NativeConfigSchemaProperty
{
    /// <summary>
    /// Supported values for <see cref="Type"/>.
    /// </summary>
    public static class SupportedTypes
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
    /// Type of the setting; one of <c>bool</c>, <c>int</c>, <c>float</c>, <c>double</c>, <c>string</c>
    /// or the name of an enum declared in the same configuration.
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
    /// Default value of the setting (bool/int/float/double, string or enum member name).
    /// Used when the user has not changed the setting, and by the Reset button.
    /// </summary>
    public object? DefaultValue { get; set; }

    /// <summary>
    /// Renders this setting as a slider. Only valid for numeric types.
    /// </summary>
    public NativeConfigSchemaSlider? Slider { get; set; }

    /// <summary>
    /// Renders this setting (string) with a file picker dialog.
    /// </summary>
    public NativeConfigSchemaFilePicker? FilePicker { get; set; }

    /// <summary>
    /// Renders this setting (string) with a folder picker dialog.
    /// </summary>
    public NativeConfigSchemaFolderPicker? FolderPicker { get; set; }

    /// <summary>
    /// Enum values declared directly on the property, for the common case where an enum is used once.
    /// </summary>
    public List<NativeConfigSchemaEnumMember> Values { get; set; } = new();

    public static NativeConfigSchemaProperty Parse(JsonNode node)
    {
        var property = new NativeConfigSchemaProperty
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
            property.Slider = NativeConfigSchemaSlider.Parse(slider);

        if (node[Keys.FilePicker] is JsonNode filePicker)
            property.FilePicker = NativeConfigSchemaFilePicker.Parse(filePicker);

        if (node[Keys.FolderPicker] is JsonNode folderPicker)
            property.FolderPicker = NativeConfigSchemaFolderPicker.Parse(folderPicker);

        if (node[Keys.Values] is JsonArray values)
        {
            foreach (var valueNode in values)
            {
                var member = valueNode!.GetValueKind() == JsonValueKind.String
                    ? new NativeConfigSchemaEnumMember { Name = valueNode.GetValue<string>() }
                    : NativeConfigSchemaEnumMember.Parse(valueNode);

                if (member.Name.Length > 0)
                    property.Values.Add(member);
            }

            if (property.Values.Count > 0)
                property.Type = property.Name; // inline enums borrow the property name.
        }

        if (property.Name.Length <= 0)
            throw new JsonException($"A property in the schema has no '{Keys.Name}'.");

        return property;
    }
}

/// <summary>
/// Parameters for the slider control; mirrors <c>SliderControlParamsAttribute</c> of the C# interface.
/// </summary>
public class NativeConfigSchemaSlider
{
    public double Minimum { get; set; } = 0.0;
    public double Maximum { get; set; } = 1.0;
    public double SmallChange { get; set; } = 0.1;
    public double LargeChange { get; set; } = 1.0;
    public int TickFrequency { get; set; } = 10;
    public bool IsSnapToTickEnabled { get; set; } = false;
    public string TickPlacement { get; set; } = "None";
    public bool ShowTextField { get; set; } = false;
    public bool IsTextFieldEditable { get; set; } = true;
    public string TextValidationRegex { get; set; } = ".*";
    public string TextFieldFormat { get; set; } = "";
    public double TickFrequencyDouble { get; set; } = 0.0;

    public static NativeConfigSchemaSlider Parse(JsonNode node) => new()
    {
        Minimum              = node.GetDoubleOrDefault(Keys.Minimum, 0.0),
        Maximum              = node.GetDoubleOrDefault(Keys.Maximum, 1.0),
        SmallChange          = node.GetDoubleOrDefault(Keys.SmallChange, 0.1),
        LargeChange          = node.GetDoubleOrDefault(Keys.LargeChange, 1.0),
        TickFrequency        = node.GetIntOrDefault(Keys.TickFrequency, 10),
        IsSnapToTickEnabled  = node.GetBoolOrDefault(Keys.IsSnapToTickEnabled, false),
        TickPlacement        = node.GetStringOrDefault(Keys.TickPlacement, "None")!,
        ShowTextField        = node.GetBoolOrDefault(Keys.ShowTextField, false),
        IsTextFieldEditable  = node.GetBoolOrDefault(Keys.IsTextFieldEditable, true),
        TextValidationRegex  = node.GetStringOrDefault(Keys.TextValidationRegex, ".*")!,
        TextFieldFormat      = node.GetStringOrDefault(Keys.TextFieldFormat, "")!,
        TickFrequencyDouble  = node.GetDoubleOrDefault(Keys.TickFrequencyDouble, 0.0)
    };
}

/// <summary>
/// Parameters for the file picker control; mirrors <c>FilePickerParamsAttribute</c> of the C# interface.
/// </summary>
public class NativeConfigSchemaFilePicker
{
    public string? InitialDirectory { get; set; }
    public int InitialFolderPath { get; set; } = 0x05; // Environment.SpecialFolder.Personal
    public string ChooseFileButtonLabel { get; set; } = "Choose File";
    public bool UserCanEditPathText { get; set; } = true;
    public string Title { get; set; } = "";
    public string Filter { get; set; } = "All files (*.*)|*.*";
    public int FilterIndex { get; set; } = 0;
    public bool Multiselect { get; set; } = false;
    public bool SupportMultiDottedExtensions { get; set; } = false;
    public bool ShowHiddenFiles { get; set; } = false;
    public bool ShowPreview { get; set; } = false;
    public bool RestoreDirectory { get; set; } = false;
    public bool AddToRecent { get; set; } = false;

    public static NativeConfigSchemaFilePicker Parse(JsonNode node) => new()
    {
        InitialDirectory           = node.GetStringOrDefault(Keys.InitialDirectory, null),
        InitialFolderPath          = node.GetIntOrDefault(Keys.InitialFolderPath, 0x05),
        ChooseFileButtonLabel      = node.GetStringOrDefault(Keys.ChooseFileButtonLabel, "Choose File")!,
        UserCanEditPathText        = node.GetBoolOrDefault(Keys.UserCanEditPathText, true),
        Title                      = node.GetStringOrDefault(Keys.Title, "")!,
        Filter                     = node.GetStringOrDefault(Keys.Filter, "All files (*.*)|*.*")!,
        FilterIndex                = node.GetIntOrDefault(Keys.FilterIndex, 0),
        Multiselect                = node.GetBoolOrDefault(Keys.Multiselect, false),
        SupportMultiDottedExtensions = node.GetBoolOrDefault(Keys.SupportMultiDottedExtensions, false),
        ShowHiddenFiles            = node.GetBoolOrDefault(Keys.ShowHiddenFiles, false),
        ShowPreview                = node.GetBoolOrDefault(Keys.ShowPreview, false),
        RestoreDirectory           = node.GetBoolOrDefault(Keys.RestoreDirectory, false),
        AddToRecent                = node.GetBoolOrDefault(Keys.AddToRecent, false)
    };
}

/// <summary>
/// Parameters for the folder picker control; mirrors <c>FolderPickerParamsAttribute</c> of the C# interface.
/// </summary>
public class NativeConfigSchemaFolderPicker
{
    public string? InitialDirectory { get; set; }
    public int InitialFolderPath { get; set; } = 0x05; // Environment.SpecialFolder.Personal
    public string ChooseFolderButtonLabel { get; set; } = "Choose Folder";
    public bool UserCanEditPathText { get; set; } = true;
    public string Title { get; set; } = "";
    public string OkButtonLabel { get; set; } = "Ok";
    public string FileNameLabel { get; set; } = "";
    public bool Multiselect { get; set; } = false;
    public bool ForceFileSystem { get; set; } = false;

    public static NativeConfigSchemaFolderPicker Parse(JsonNode node) => new()
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

/// <summary>
/// JSON property names used by the schema file.
/// </summary>
internal static class Keys
{
    public const string Configurations = "Configurations";
    public const string FileName       = "FileName";
    public const string DisplayName    = "DisplayName";
    public const string Enums          = "Enums";
    public const string Properties     = "Properties";
    public const string Members        = "Members";
    public const string Name           = "Name";
    public const string Type           = "Type";
    public const string Description    = "Description";
    public const string Category       = "Category";
    public const string Order          = "Order";
    public const string DefaultValue   = "DefaultValue";
    public const string Slider         = "Slider";
    public const string FilePicker     = "FilePicker";
    public const string FolderPicker   = "FolderPicker";
    public const string Values         = "Values";

    // Control Params
    public const string Minimum              = "Minimum";
    public const string Maximum              = "Maximum";
    public const string SmallChange          = "SmallChange";
    public const string LargeChange          = "LargeChange";
    public const string TickFrequency        = "TickFrequency";
    public const string TickFrequencyDouble  = "TickFrequencyDouble";
    public const string IsSnapToTickEnabled  = "IsSnapToTickEnabled";
    public const string TickPlacement        = "TickPlacement";
    public const string ShowTextField        = "ShowTextField";
    public const string IsTextFieldEditable  = "IsTextFieldEditable";
    public const string TextValidationRegex  = "TextValidationRegex";
    public const string TextFieldFormat      = "TextFieldFormat";
    public const string InitialDirectory     = "InitialDirectory";
    public const string InitialFolderPath    = "InitialFolderPath";
    public const string ChooseFileButtonLabel   = "ChooseFileButtonLabel";
    public const string ChooseFolderButtonLabel = "ChooseFolderButtonLabel";
    public const string UserCanEditPathText  = "UserCanEditPathText";
    public const string Title                = "Title";
    public const string Filter               = "Filter";
    public const string FilterIndex          = "FilterIndex";
    public const string Multiselect          = "Multiselect";
    public const string SupportMultiDottedExtensions = "SupportMultiDottedExtensions";
    public const string ShowHiddenFiles      = "ShowHiddenFiles";
    public const string ShowPreview          = "ShowPreview";
    public const string RestoreDirectory     = "RestoreDirectory";
    public const string AddToRecent          = "AddToRecent";
    public const string OkButtonLabel        = "OkButtonLabel";
    public const string FileNameLabel        = "FileNameLabel";
    public const string ForceFileSystem      = "ForceFileSystem";
}

/// <summary>
/// Helper extensions for reading values out of <see cref="JsonNode"/>s.
/// </summary>
internal static class JsonNodeExtensions
{
    public static string? GetStringOrDefault(this JsonNode? node, string name, string? fallback)
    {
        var value = node[name];
        if (value == null)
            return fallback;

        return value.GetValueKind() == JsonValueKind.String ? value.GetValue<string>() : fallback;
    }

    public static int GetIntOrDefault(this JsonNode? node, string name, int fallback)
    {
        var value = node[name];
        if (value == null)
            return fallback;

        if (value.GetValueKind() == JsonValueKind.Number)
        {
            var element = value.GetValue<JsonElement>();
            if (element.TryGetInt32(out var result))
                return result;
        }

        return fallback;
    }

    public static int? GetIntOrNull(this JsonNode? node, string name)
    {
        var value = node[name];
        if (value == null)
            return null;

        if (value.GetValueKind() == JsonValueKind.Number)
        {
            var element = value.GetValue<JsonElement>();
            if (element.TryGetInt32(out var result))
                return result;
        }

        return null;
    }

    public static double GetDoubleOrDefault(this JsonNode? node, string name, double fallback)
    {
        var value = node[name];
        if (value == null)
            return fallback;

        return value.GetValueKind() == JsonValueKind.Number ? value.GetValue<JsonElement>().GetDouble() : fallback;
    }

    public static bool GetBoolOrDefault(this JsonNode? node, string name, bool fallback)
    {
        var value = node[name];
        if (value == null)
            return fallback;

        return value.GetValueKind() == JsonValueKind.True || value.GetValueKind() == JsonValueKind.False ? value.GetValue<bool>() : fallback;
    }

    /// <summary>
    /// Returns the raw boxed value of a node (bool/int/double/string) or null.
    /// </summary>
    public static object? GetValueOrNull(this JsonNode? node)
    {
        if (node == null)
            return null;

        var kind = node.GetValueKind();
        if (kind == JsonValueKind.True || kind == JsonValueKind.False)
            return node.GetValue<bool>();

        if (kind == JsonValueKind.Number)
        {
            var element = node.GetValue<JsonElement>();
            return element.TryGetInt32(out var i) ? i : element.GetDouble();
        }

        if (kind == JsonValueKind.String)
            return node.GetValue<string>();

        return null;
    }

    public static JsonValueKind GetValueKind(this JsonNode node) => node.GetValue<JsonElement>().ValueKind;
}
