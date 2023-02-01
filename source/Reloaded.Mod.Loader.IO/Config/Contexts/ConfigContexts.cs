namespace Reloaded.Mod.Loader.IO.Config.Contexts;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(LoaderConfig))]
internal partial class LoaderConfigContext : JsonSerializerContext { }

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(ApplicationConfig))]
internal partial class ApplicationConfigContext : JsonSerializerContext { }

[JsonSerializable(typeof(ModConfig))]
internal partial class ModConfigContext : JsonSerializerContext { }

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(ModSet))]
internal partial class ModSetContext : JsonSerializerContext { }

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(ModUserConfig))]
internal partial class ModUserConfigContext : JsonSerializerContext { }