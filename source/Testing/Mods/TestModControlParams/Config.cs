using System.ComponentModel;
using TestModControlParams.Template.Configuration;
using Reloaded.Mod.Interfaces.Structs;

namespace TestModControlParams.Configuration
{
    public class Config : Configurable<Config>
    {
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
        [SliderControlParams(minimum: 0.0, maximum: 1.0, showTextField: true, textFieldFormat: "{0:#,0.000}")]
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
}
