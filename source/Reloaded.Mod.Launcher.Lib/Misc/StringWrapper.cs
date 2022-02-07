namespace Reloaded.Mod.Launcher.Lib.Misc;

/// <summary>
/// Class that wraps a string. Used for data binding.
/// </summary>
public class StringWrapper
{
    /// <summary>
    /// Value of the string wrapper.
    /// </summary>
    public string Value { get; set; } = "";

    /// <summary/>
    public static implicit operator string(StringWrapper wrapper) => wrapper.Value;

    /// <summary/>
    public static implicit operator StringWrapper(string value) => new() { Value = value };
}