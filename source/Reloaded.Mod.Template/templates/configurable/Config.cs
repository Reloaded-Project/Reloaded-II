#if (IncludeConfig)
using System.ComponentModel;
using Reloaded.Mod.Template.Template.Configuration;
using Reloaded.Mod.Interfaces.Structs;

namespace Reloaded.Mod.Template.Configuration;

public class Config : Configurable<Config>
{
    /*
        User Properties:
            - Please put all of your configurable properties here.
    
        By default, configuration saves as "Config.json" in mod user config folder.    
        Need more config files/classes? See Configuration.cs
    
        Available Attributes:
        - Category
        - DisplayName
        - Description
        - DefaultValue

        // Technically Supported but not Useful
        - Browsable
        - Localizable

        The `DefaultValue` attribute is used as part of the `Reset` button in Reloaded-Launcher.
    */

    [DisplayName("String")]
    [Description("This is a string.")]
    [DefaultValue("Default Name")]
    public string String { get; set; } = "Default Name";

    [DisplayName("Int")]
    [Description("This is an int.")]
    [DefaultValue(42)]
    public int Integer { get; set; } = 42;

    [DisplayName("Bool")]
    [Description("This is a bool.")]
    [DefaultValue(true)]
    public bool Boolean { get; set; } = true;

    [DisplayName("Float")]
    [Description("This is a floating point number.")]
    [DefaultValue(6.987654F)]
    public float Float { get; set; } = 6.987654F;

    [DisplayName("Enum")]
    [Description("This is an enumerable.")]
    [DefaultValue(SampleEnum.ILoveIt)]
    public SampleEnum Reloaded { get; set; } = SampleEnum.ILoveIt;

    public enum SampleEnum
    {
        NoOpinion,
        Sucks,
        IsMediocre,
        IsOk,
        IsCool,
        ILoveIt
    }
    
    [DisplayName("Int Slider")]
    [Description("This is a int that uses a slider control similar to a volume control slider.")]
    [DefaultValue(100)]
    [SliderControlParams(
        minimum: 0.0,
        maximum: 100.0,
        smallChange: 1.0,
        largeChange: 10.0,
        tickFrequency: 10,
        isSnapToTickEnabled: false,
        tickPlacement:SliderControlTickPlacement.BottomRight,
        showTextField: true,
        isTextFieldEditable: true,
        textValidationRegex: "\\d{1-3}")]
    public int IntSlider { get; set; } = 100;

    [DisplayName("Double Slider")]
    [Description("This is a double that uses a slider control without any frills.")]
    [DefaultValue(0.5)]
    [SliderControlParams(minimum: 0.0, maximum: 1.0)]
    public double DoubleSlider { get; set; } = 0.5;

    [DisplayName("File Picker")]
    [Description("This is a sample file picker.")]
    [DefaultValue("")]
    [FilePickerParams(title:"Choose a File to load from")]
    public string File { get; set; } = "";

    [DisplayName("Folder Picker")]
    [Description("Opens a file picker but locked to only allow folder selections.")]
    [DefaultValue("")]
    [FolderPickerParams(
        initialFolderPath: Environment.SpecialFolder.Desktop,
        userCanEditPathText: false,
        title: "Custom Folder Select",
        okButtonLabel: "Choose Folder",
        fileNameLabel: "ModFolder",
        multiSelect: true,
        forceFileSystem: true)]
    public string Folder { get; set; } = "";
}

/// <summary>
/// Allows you to override certain aspects of the configuration creation process (e.g. create multiple configurations).
/// Override elements in <see cref="ConfiguratorMixinBase"/> for finer control.
/// </summary>
public class ConfiguratorMixin : ConfiguratorMixinBase
{
    // 
}
#endif