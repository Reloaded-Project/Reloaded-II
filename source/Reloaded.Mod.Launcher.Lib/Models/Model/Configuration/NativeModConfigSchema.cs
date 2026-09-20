using System.Text.Json.Nodes;

namespace Reloaded.Mod.Launcher.Lib.Models.Model.Configuration;

/// <summary>
/// Declarative configuration schema for native (non .NET) mods.
/// A mod declares its settings in a <c>ConfigSchema.json</c> file next to
/// its <c>ModConfig.json</c>. The launcher builds the configuration UI
/// from that schema.
///
/// The schema mirrors the attributes used by the C# mod template.
/// Native and C# mods therefore look and behave the same:
/// - DisplayName, Description, Category, DefaultValue
/// - Slider/File/Folder control params
/// </summary>
public class NativeModConfigSchema
{
    /// <summary>
    /// Name of the config file to place inside the mod folder.
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
    /// <exception cref="InvalidOperationException">
    /// Thrown when the schema is empty or invalid. A missing file throws
    /// <see cref="FileNotFoundException"/>; malformed JSON,
    /// <see cref="JsonException"/>.
    /// </exception>
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
/// Individual configuration of a native mod; essentially mirrors one
/// <c>IConfigurable</c> from the C# mod template.
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

    /// <summary>
    /// Reads a configuration from its JSON representation.
    /// </summary>
    /// <param name="node">Node holding the configuration's properties.</param>
    /// <exception cref="JsonException">
    /// Thrown when <c>FileName</c> is not a plain file name.
    /// </exception>
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

    /// <summary>
    /// Reads an enum from its JSON representation.
    /// </summary>
    /// <param name="node">Node holding the enum's properties.</param>
    /// <exception cref="JsonException">
    /// Thrown when the enum declares no members.
    /// </exception>
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

    /// <summary>
    /// Reads an enum member from its JSON representation.
    /// </summary>
    /// <param name="node">Node holding the member's properties.</param>
    public static NativeConfigSchemaEnumMember Parse(JsonNode node) => new()
    {
        Name        = node.GetStringOrDefault(Keys.Name, "")!,
        DisplayName = node.GetStringOrDefault(Keys.DisplayName, null)
    };
}

/// <summary>
/// An individual setting of a configuration; mirrors a property of a
/// C# mod's config class.
/// </summary>
public class NativeConfigSchemaProperty
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
    /// Enum values declared directly on the property, for the common case
    /// of an enum used by a single setting.
    /// </summary>
    public List<NativeConfigSchemaEnumMember> Values { get; set; } = new();

    /// <summary>
    /// Reads a property from its JSON representation.
    /// </summary>
    /// <param name="node">Node holding the setting's properties.</param>
    /// <exception cref="JsonException">
    /// Thrown when the property has no name.
    /// </exception>
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
                property.Type = property.Name; // inline enums uses the property name.
        }

        if (property.Name.Length <= 0)
            throw new JsonException($"A property in the schema has no '{Keys.Name}'.");

        return property;
    }
}

/// <summary>
/// Parameters for the slider control; mirrors
/// <c>SliderControlParamsAttribute</c> of the C# interface.
/// </summary>
public class NativeConfigSchemaSlider
{
    /// <summary>
    /// Minimum value of the slider.
    /// </summary>
    public double Minimum { get; set; } = 0.0;

    /// <summary>
    /// Maximum value of the slider.
    /// </summary>
    public double Maximum { get; set; } = 1.0;

    /// <summary>
    /// Value change of a small step (arrow keys).
    /// </summary>
    public double SmallChange { get; set; } = 0.1;

    /// <summary>
    /// Value change of a large step (page up/down or gutter click).
    /// </summary>
    public double LargeChange { get; set; } = 1.0;

    /// <summary>
    /// Distance between tick marks. Legacy;
    /// <see cref="TickFrequencyDouble"/> wins when greater than zero.
    /// </summary>
    public int TickFrequency { get; set; } = 10;

    /// <summary>
    /// Snap the value to the nearest tick.
    /// </summary>
    public bool IsSnapToTickEnabled { get; set; } = false;

    /// <summary>
    /// Where tick marks are drawn; a <c>SliderControlTickPlacement</c> name.
    /// </summary>
    public string TickPlacement { get; set; } = "None";

    /// <summary>
    /// Show the value in a text field left of the slider.
    /// </summary>
    public bool ShowTextField { get; set; } = false;

    /// <summary>
    /// Allow typing in the text field.
    /// </summary>
    public bool IsTextFieldEditable { get; set; } = true;

    /// <summary>
    /// Regex the text field input must match.
    /// </summary>
    public string TextValidationRegex { get; set; } = ".*";

    /// <summary>
    /// Format string applied to the text field value.
    /// </summary>
    public string TextFieldFormat { get; set; } = "";

    /// <summary>
    /// Distance between tick marks; allows fractions.
    /// </summary>
    public double TickFrequencyDouble { get; set; } = 0.0;

    /// <summary>
    /// Reads the slider parameters from their JSON representation.
    /// </summary>
    /// <param name="node">Node holding the slider's properties.</param>
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
/// Parameters for the file picker control; mirrors
/// <c>FilePickerParamsAttribute</c> of the C# interface.
/// </summary>
public class NativeConfigSchemaFilePicker
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
/// Parameters for the folder picker control; mirrors
/// <c>FolderPickerParamsAttribute</c> of the C# interface.
/// </summary>
public class NativeConfigSchemaFolderPicker
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
        var value = node?[name];
        if (value == null)
            return fallback;

        return value.GetValueKind() == JsonValueKind.String ? value.GetValue<string>() : fallback;
    }

    public static int GetIntOrDefault(this JsonNode? node, string name, int fallback)
    {
        var value = node?[name];
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
        var value = node?[name];
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
        var value = node?[name];
        if (value == null)
            return fallback;

        return value.GetValueKind() == JsonValueKind.Number ? value.GetValue<JsonElement>().GetDouble() : fallback;
    }

    public static bool GetBoolOrDefault(this JsonNode? node, string name, bool fallback)
    {
        var value = node?[name];
        if (value == null)
            return fallback;

        return value.GetValueKind() == JsonValueKind.True || value.GetValueKind() == JsonValueKind.False ? value.GetValue<bool>() : fallback;
    }

    /// <summary>
    /// Returns the raw boxed value of a node as one of the following:
    /// - <c>bool</c>
    /// - <c>int</c> if it fits, else <c>double</c>
    /// - <c>string</c>
    /// - null for any other content
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
