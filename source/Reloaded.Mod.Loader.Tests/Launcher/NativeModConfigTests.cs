using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text.Json.Nodes;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Interfaces.Structs;
using Reloaded.Mod.Launcher.Lib.Models.Model.Configuration;

namespace Reloaded.Mod.Loader.Tests.Launcher;

/// <summary>
/// Tests the schema driven configuration of native (C/C++) mods.
/// </summary>
public class NativeModConfigTests : IDisposable
{
    private const string Schema = """
    {
      "Configurations": [
        {
          "FileName": "Config.json",
          "DisplayName": "Default Config",
          "Enums": [
            {
              "Name": "SampleEnum",
              "Members": [ { "Name": "NoOpinion" }, { "Name": "ILoveIt", "DisplayName": "I Love It!!!" } ]
            }
          ],
          "Properties": [
            { "Name": "BooleanSetting", "Type": "bool", "DisplayName": "Bool", "Description": "This is a bool.", "Category": "Cat A", "Order": 1, "DefaultValue": true },
            { "Name": "IntegerSetting", "Type": "int", "DefaultValue": 42, "Order": 2 },
            { "Name": "FloatSetting", "Type": "float", "DefaultValue": 6.5 },
            { "Name": "StringSetting", "Type": "string", "DefaultValue": "hello world" },
            { "Name": "EnumSetting", "Type": "SampleEnum", "DefaultValue": "ILoveIt" },
            {
              "Name": "SliderSetting", "Type": "int", "DefaultValue": 100, "Order": 0,
              "Slider": { "Minimum": 0.0, "Maximum": 100.0, "SmallChange": 1.0, "LargeChange": 10.0, "TickFrequency": 10, "TickFrequencyDouble": 2.5, "ShowTextField": true }
            },
            { "Name": "FileSetting", "Type": "string", "DefaultValue": "", "FilePicker": { "Title": "Pick a file", "Filter": "Text (*.txt)|*.txt" } }
          ]
        }
      ]
    }
    """;

    private string ModDirectory { get; }
    private string ConfigDirectory { get; }

    public NativeModConfigTests()
    {
        ModDirectory = Path.Combine(Path.GetTempPath(), $"reloaded-native-mod-{Guid.NewGuid():N}");
        ConfigDirectory = Path.Combine(Path.GetTempPath(), $"reloaded-native-config-{Guid.NewGuid():N}");
        Directory.CreateDirectory(ModDirectory);
        Directory.CreateDirectory(ConfigDirectory);
        File.WriteAllText(Path.Combine(ModDirectory, NativeModConfigSchema.SchemaFileName), Schema);
    }

    [Fact]
    public void Schema_Is_Detected_And_Parsed()
    {
        Assert.True(NativeModConfigSchema.ExistsInFolder(ModDirectory));

        var schema = NativeModConfigSchema.Load(ModDirectory);
        var configuration = Assert.Single(schema.Configurations);
        Assert.Equal("Config.json", configuration.FileName);
        Assert.Equal("Default Config", configuration.DisplayName);
        Assert.Equal(7, configuration.Properties.Count);
        Assert.Single(configuration.Enums);

        var sliderProperty = configuration.Properties.First(p => p.Name == "SliderSetting");
        Assert.NotNull(sliderProperty.Slider);
        Assert.Equal(0.0, sliderProperty.Slider!.Minimum);
        Assert.Equal(100.0, sliderProperty.Slider!.Maximum);

        var fileProperty = configuration.Properties.First(p => p.Name == "FileSetting");
        Assert.Equal("Text (*.txt)|*.txt", fileProperty.FilePicker!.Filter);
    }

    [Fact]
    public void Configurator_Returns_Configurable_With_Default_Values()
    {
        var configurator = CreateConfigurator();
        var configurations = configurator.GetConfigurations();
        var configurable = Assert.Single(configurations);

        Assert.Equal("Default Config", configurable.ConfigName);
        Assert.IsAssignableFrom<IUpdatableConfigurable>(configurable);
        Assert.NotNull(configurable.Save);

        Assert.True(GetProperty<bool>(configurable, "BooleanSetting"));
        Assert.Equal(42, GetProperty<int>(configurable, "IntegerSetting"));
        Assert.Equal(6.5f, GetProperty<float>(configurable, "FloatSetting"));
        Assert.Equal("hello world", GetProperty<string>(configurable, "StringSetting"));
        Assert.Equal("ILoveIt", GetProperty<object>(configurable, "EnumSetting")!.ToString());
        Assert.Equal(100, GetProperty<int>(configurable, "SliderSetting"));
    }

    [Fact]
    public void Generated_Properties_Carry_UI_Attributes()
    {
        var configurable = Assert.Single(CreateConfigurator().GetConfigurations());
        var type = configurable.GetType();

        var booleanProperty = type.GetProperty("BooleanSetting")!;
        Assert.Equal("Bool", booleanProperty.GetCustomAttribute<DisplayNameAttribute>()!.DisplayName);
        Assert.Equal("This is a bool.", booleanProperty.GetCustomAttribute<DescriptionAttribute>()!.Description);
        Assert.Equal("Cat A", booleanProperty.GetCustomAttribute<CategoryAttribute>()!.Category);
        Assert.Equal(true, booleanProperty.GetCustomAttribute<DefaultValueAttribute>()!.Value);

        var display = type.GetProperty("SliderSetting")!.GetCustomAttribute<DisplayAttribute>();
        Assert.Equal(0, display!.Order);

        var slider = type.GetProperty("SliderSetting")!.GetCustomAttribute<SliderControlParamsAttribute>();
        Assert.NotNull(slider);
        Assert.Equal(0.0, slider!.Minimum);
        Assert.Equal(100.0, slider.Maximum);
#pragma warning disable CS0618 // Legacy tick frequency; schemas still feed it.
        Assert.Equal(10, slider.TickFrequency);
#pragma warning restore CS0618

        var filePicker = type.GetProperty("FileSetting")!.GetCustomAttribute<FilePickerParamsAttribute>();
        Assert.NotNull(filePicker);
        Assert.Equal("Text (*.txt)|*.txt", filePicker!.Filter);

        // Enum members support display names.
        var enumProperty = type.GetProperty("EnumSetting")!;
        var enumType = enumProperty.PropertyType;
        Assert.True(enumType.IsEnum);

        Assert.Null(enumProperty.GetCustomAttribute<DefaultValueAttribute>());
        var members = enumType.GetFields(BindingFlags.Public | BindingFlags.Static);
        Assert.Equal(2, members.Length);
        Assert.Equal("I Love It!!!", members[1].GetCustomAttribute<DisplayAttribute>()?.GetName());
    }

    [Fact]
    public void Save_Writes_Values_And_New_Instance_Reads_Them_Back()
    {
        var configurable = Assert.Single(CreateConfigurator().GetConfigurations());
        SetProperty(configurable, "BooleanSetting", false);
        SetProperty(configurable, "IntegerSetting", 1337);
        SetProperty(configurable, "FloatSetting", 0.25f);
        SetProperty(configurable, "StringSetting", "changed");
        SetProperty(configurable, "EnumSetting", Enum.Parse(configurable.GetType().GetProperty("EnumSetting")!.PropertyType, "NoOpinion"));
        configurable.Save!();

        string valuesPath = Path.Combine(ConfigDirectory, "Config.json");
        Assert.True(File.Exists(valuesPath));

        // The file is flat JSON with enums as strings, easy to parse from C/C++.
        var json = JsonNode.Parse(File.ReadAllText(valuesPath))!;
        Assert.False(json["BooleanSetting"]!.GetValue<bool>());
        Assert.Equal(1337, json["IntegerSetting"]!.GetValue<int>());
        Assert.Equal(0.25, json["FloatSetting"]!.GetValue<double>(), 5);
        Assert.Equal("changed", json["StringSetting"]!.GetValue<string>());
        Assert.Equal("NoOpinion", json["EnumSetting"]!.GetValue<string>());

        // A fresh instance starts from the saved values.
        var reloaded = Assert.Single(CreateConfigurator().GetConfigurations());
        Assert.False(GetProperty<bool>(reloaded, "BooleanSetting"));
        Assert.Equal(1337, GetProperty<int>(reloaded, "IntegerSetting"));
        Assert.Equal(0.25f, GetProperty<float>(reloaded, "FloatSetting"));
        Assert.Equal("changed", GetProperty<string>(reloaded, "StringSetting"));
        Assert.Equal("NoOpinion", GetProperty<object>(reloaded, "EnumSetting")!.ToString());
    }

    [Fact]
    public void Unknown_Values_In_File_Are_Ignored()
    {
        File.WriteAllText(Path.Combine(ConfigDirectory, "Config.json"), """{ "IntegerSetting": 5, "NotARealSetting": "abc" }""");
        var configurable = Assert.Single(CreateConfigurator().GetConfigurations());

        Assert.Equal(5, GetProperty<int>(configurable, "IntegerSetting"));
        Assert.True(GetProperty<bool>(configurable, "BooleanSetting"));
        Assert.Equal("hello world", GetProperty<string>(configurable, "StringSetting"));
    }

    [Fact]
    public void Missing_Values_File_Leaves_Defaults()
    {
        var configurable = Assert.Single(CreateConfigurator().GetConfigurations());
        Assert.Equal(42, GetProperty<int>(configurable, "IntegerSetting"));
        Assert.False(File.Exists(Path.Combine(ConfigDirectory, "Config.json")));
    }

    [Fact]
    public void Migrate_Moves_Values_File()
    {
        // Simulate values living in the mod folder (pre-migration).
        File.WriteAllText(Path.Combine(ModDirectory, "Config.json"), """{ "IntegerSetting": 9 }""");

        var configurator = CreateConfigurator();
        configurator.Migrate(ModDirectory, ConfigDirectory);

        Assert.False(File.Exists(Path.Combine(ModDirectory, "Config.json")));
        Assert.True(File.Exists(Path.Combine(ConfigDirectory, "Config.json")));

        var configurable = Assert.Single(configurator.GetConfigurations());
        Assert.Equal(9, GetProperty<int>(configurable, "IntegerSetting"));
    }

    [Fact]
    public void Unknown_Type_Throws_Descriptive_Error()
    {
        File.WriteAllText(Path.Combine(ModDirectory, NativeModConfigSchema.SchemaFileName), """
        { "Configurations": [ { "FileName": "Config.json", "Properties": [ { "Name": "Broken", "Type": "NoSuchEnum" } ] } ] }
        """);
        var configurator = CreateConfigurator();

        var error = Assert.Throws<InvalidOperationException>(() => configurator.GetConfigurations());
        Assert.Contains("nosuchenum", error.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Slider_On_Enum_Property_Throws()
    {
        File.WriteAllText(Path.Combine(ModDirectory, NativeModConfigSchema.SchemaFileName), """
        {
          "Configurations": [
          {
            "FileName": "Config.json",
            "Enums": [ { "Name": "Mode", "Members": [ { "Name": "Fast" }, { "Name": "Slow" } ] } ],
            "Properties": [ { "Name": "Speed", "Type": "Mode", "DefaultValue": "Fast", "Slider": { "Minimum": 0.0, "Maximum": 1.0 } } ]
          }]
        }
        """);
        var configurator = CreateConfigurator();

        var error = Assert.Throws<InvalidOperationException>(() => configurator.GetConfigurations());
        Assert.Contains("sliders are only supported", error.Message);
    }

    [Theory]
    [InlineData("../evil.json")]
    [InlineData("..\\evil.json")]
    [InlineData("C:\\Windows\\Temp\\evil.json")]
    [InlineData("SubFolder/Config.json")]
    public void FileNames_With_Paths_Are_Rejected(string fileName)
    {
        File.WriteAllText(Path.Combine(ModDirectory, NativeModConfigSchema.SchemaFileName), $$"""
        { "Configurations": [ { "FileName": "{{fileName.Replace("\\", "\\\\")}}", "Properties": [] } ] }
        """);

        var error = Assert.Throws<InvalidOperationException>(() => NativeModConfigSchema.Load(ModDirectory));
        var jsonError = Assert.IsType<JsonException>(error.InnerException);
        Assert.Contains("plain file name", jsonError.Message);
    }

    [Fact]
    public void TryMigrate_Reports_Failure_And_Keeps_Error()
    {
        var configurator = CreateConfigurator();

        // A path with invalid characters makes creating the directory fail.
        Assert.False(configurator.TryMigrate(ModDirectory, "C:\\<not a valid folder>\\"));
        Assert.NotNull(configurator.MigrationError);
    }

    [Fact]
    public void TryMigrate_Rolls_Back_Moves_On_Failure()
    {
        // Two configs with values in the mod folder; the second move fails
        // because a directory already sits where the file would land.
        File.WriteAllText(Path.Combine(ModDirectory, NativeModConfigSchema.SchemaFileName), """
        {
          "Configurations": [
            { "FileName": "First.json", "Properties": [] },
            { "FileName": "Second.json", "Properties": [] }
          ]
        }
        """);
        File.WriteAllText(Path.Combine(ModDirectory, "First.json"), "{ \"Value\": 1 }");
        File.WriteAllText(Path.Combine(ModDirectory, "Second.json"), "{ \"Value\": 2 }");
        Directory.CreateDirectory(Path.Combine(ConfigDirectory, "Second.json"));

        var configurator = CreateConfigurator();
        Assert.False(configurator.TryMigrate(ModDirectory, ConfigDirectory));
        Assert.NotNull(configurator.MigrationError);

        // The first file was moved before the failure: put it back in place.
        Assert.True(File.Exists(Path.Combine(ModDirectory, "First.json")));
        Assert.True(File.Exists(Path.Combine(ModDirectory, "Second.json")));
        Assert.False(File.Exists(Path.Combine(ConfigDirectory, "First.json")));
    }

    [Fact]
    public void Inline_Enum_Values_Build_A_Dropdown()
    {
        File.WriteAllText(Path.Combine(ModDirectory, NativeModConfigSchema.SchemaFileName), """
        {
          "Configurations": [
          {
            "FileName": "Config.json",
            "Properties": [
              {
                "Name": "Difficulty",
                "Type": "enum",
                "DefaultValue": "Hard",
                "Values": [ "Easy", { "Name": "Hard", "DisplayName": "Very Hard" } ]
              }
            ]
          }]
        }
        """);
        var configurable = Assert.Single(CreateConfigurator().GetConfigurations());

        var property = configurable.GetType().GetProperty("Difficulty")!;
        var enumType = property.PropertyType;
        Assert.True(enumType.IsEnum);
        Assert.Equal("Hard", GetProperty<object>(configurable, "Difficulty")!.ToString());

        var members = enumType.GetFields(BindingFlags.Public | BindingFlags.Static);
        Assert.Equal(2, members.Length);
        Assert.Equal("Easy", members[0].Name);
        Assert.Equal("Very Hard", members[1].GetCustomAttribute<DisplayAttribute>()?.GetName());
    }

    [Fact]
    public void Enum_Type_Without_Values_Gives_Hint()
    {
        File.WriteAllText(Path.Combine(ModDirectory, NativeModConfigSchema.SchemaFileName), """
        { "Configurations": [ { "FileName": "Config.json", "Properties": [ { "Name": "Broken", "Type": "enum" } ] } ] }
        """);
        var configurator = CreateConfigurator();

        var error = Assert.Throws<InvalidOperationException>(() => configurator.GetConfigurations());
        Assert.Contains("Values", error.Message);
    }

    private NativeModConfigurator CreateConfigurator()
    {
        var configurator = new NativeModConfigurator(ModDirectory);
        configurator.SetModDirectory(ModDirectory);
        configurator.SetConfigDirectory(ConfigDirectory);
        return configurator;
    }

    private static T GetProperty<T>(IConfigurable configurable, string name) => (T)configurable.GetType().GetProperty(name)!.GetValue(configurable)!;

    private static void SetProperty(IConfigurable configurable, string name, object value) => configurable.GetType().GetProperty(name)!.SetValue(configurable, value);

    public void Dispose()
    {
        Directory.Delete(ModDirectory, true);
        if (Directory.Exists(ConfigDirectory))
            Directory.Delete(ConfigDirectory, true);
    }
}
