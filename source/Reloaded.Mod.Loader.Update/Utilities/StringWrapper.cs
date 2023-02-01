namespace Reloaded.Mod.Loader.Update.Utilities;

/// <summary>
/// Class that wraps a string. Used for data binding.
/// </summary>
[JsonConverter(typeof(StringWrapperConverter))]
public class StringWrapper : ObservableObject
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

/// <inheritdoc />
public class StringWrapperConverter : JsonConverter<StringWrapper>
{
    /// <inheritdoc />
    public override StringWrapper? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return new StringWrapper() { Value = reader.GetString()! };
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, StringWrapper value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.Value);
    }
}