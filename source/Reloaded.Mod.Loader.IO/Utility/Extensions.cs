using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;

namespace Reloaded.Mod.Loader.IO.Utility
{
    public static class Extensions
    {
        /// <summary>
        /// Posts the event to the synchronization context if it is available, else directly executes it.
        /// </summary>
        public static void Post(this SynchronizationContext context, Action action)
        {
            if (context != null)
                context.Post(state => action(), null);
            else
                action();
        }

        /// <summary>
        /// Retrieves an item (which may have been deserialized) from a string->object map.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pluginData">The dictionary to get the item from.</param>
        /// <param name="key">The item's key.</param>
        /// <returns>The item in question, or default if it does not exist.</returns>
        public static T TryGetValue<T>(this Dictionary<string, object> pluginData, string key)
        {
            if (!pluginData.TryGetValue(key, out var value))
                return default;

            if (value is JsonElement element)
                return JsonSerializer.Deserialize<T>(element.ToString());

            if (value is T result)
                return result;

            throw new Exception($"Cannot convert key from dictionary to specified type T ({nameof(T)})");
        }
    }
}
