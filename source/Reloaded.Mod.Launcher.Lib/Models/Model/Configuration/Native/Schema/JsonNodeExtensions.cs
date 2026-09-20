using System.Text.Json.Nodes;

namespace Reloaded.Mod.Launcher.Lib.Models.Model.Configuration.Native.Schema;

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
