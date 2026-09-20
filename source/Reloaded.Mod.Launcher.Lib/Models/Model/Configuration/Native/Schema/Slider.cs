using System.Text.Json.Nodes;

namespace Reloaded.Mod.Launcher.Lib.Models.Model.Configuration.Native.Schema;

/// <summary>
/// Parameters for the slider control; native equivalent of
/// <see cref="Reloaded.Mod.Interfaces.Structs.SliderControlParamsAttribute"/>.
/// </summary>
public class Slider
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
    /// Where tick marks are drawn; a
    /// <see cref="Reloaded.Mod.Interfaces.Structs.SliderControlTickPlacement"/> name.
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
    public static Slider Parse(JsonNode node) => new()
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
