using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

namespace Reloaded.Mod.Loader.Update.Abstract
{
    public abstract class JsonSerializable<TType>
    {
        private static JsonSerializerOptions Options = new JsonSerializerOptions() { WriteIndented = true };

        public static TType FromPath(string filePath)
        {
            string jsonFile = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<TType>(jsonFile, Options);
        }

        public static void ToPath(TType config, string filePath)
        {
            string fullPath = Path.GetFullPath(filePath);
            string directoryOfPath = Path.GetDirectoryName(fullPath);
            if (!Directory.Exists(directoryOfPath))
                Directory.CreateDirectory(directoryOfPath);

            string jsonFile = JsonSerializer.Serialize(config, Options);
            File.WriteAllText(fullPath, jsonFile);
        }
    }
}
