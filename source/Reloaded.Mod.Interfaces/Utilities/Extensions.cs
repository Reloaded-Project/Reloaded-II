namespace Reloaded.Mod.Interfaces.Utilities;

public static class Extensions
{
    /// <summary>
    /// Retrieves an item (which may have been deserialized) from a string->object map.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="pluginData">The dictionary to get the item from.</param>
    /// <param name="key">The item's key.</param>
    /// <param name="result">Stores the result of the operation.</param>
    /// <returns>True if the item can be obtained, else false.</returns>
    public static bool TryGetValue<T>(this Dictionary<string, object> pluginData, string key, out T result)
    {
        if (!pluginData.TryGetValue(key, out var value))
        {
            result = default;
            return false;
        }

        if (value is JsonElement element)
        {
            result = JsonSerializer.Deserialize<T>(element.ToString());
            return true;
        }

        if (value is T generic)
        {
            result = generic;
            return true;
        }

        throw new Exception($"Cannot convert key from dictionary to specified type T ({nameof(T)})");
    }
}