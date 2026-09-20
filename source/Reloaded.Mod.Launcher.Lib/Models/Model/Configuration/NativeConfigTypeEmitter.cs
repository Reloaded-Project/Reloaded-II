using System.Reflection.Emit;
using Reloaded.Mod.Interfaces.Structs;
using DataAnnotations = System.ComponentModel.DataAnnotations;

namespace Reloaded.Mod.Launcher.Lib.Models.Model.Configuration;

/// <summary>
/// Builds .NET types from native mod configuration schemas using
/// Reflection.Emit.
/// The generated types subclass <see cref="NativeConfigurableBase"/> and carry
/// the same attributes as a hand written C# configuration class:
/// - <see cref="DisplayNameAttribute"/>, <see cref="DescriptionAttribute"/>,
///   <see cref="CategoryAttribute"/>
/// - <see cref="DefaultValueAttribute"/> (backs the Reset button of the dialog)
/// - <c>Display</c> (sort order)
/// - <c>SliderControlParams</c>, <c>FilePickerParams</c>,
///   <c>FolderPickerParams</c> (custom editors)
/// The PropertyGrid renders them exactly like a C# mod's configuration.
/// </summary>
public static class NativeConfigTypeEmitter
{
    private static readonly object BuildLock = new object();
    private static ModuleBuilder? _module;
    private static int _typeCounter;

    private static Dictionary<string, Type> TypeCache { get; } = new Dictionary<string, Type>();

    /// <summary>
    /// Creates an instance of the configuration type for a schema configuration.
    /// Types are cached; the cache key should change whenever the schema changes.
    /// </summary>
    /// <param name="configuration">The configuration to build a type for.</param>
    /// <param name="cacheKey">Unique key identifying the (version of the) configuration.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when a property's Type is unknown, or a control does not match
    /// the property type.
    /// </exception>
    public static NativeConfigurableBase CreateInstance(NativeConfigSchemaConfiguration configuration, string cacheKey)
    {
        Type type;
        lock (BuildLock)
        {
            if (!TypeCache.TryGetValue(cacheKey, out type!))
            {
                type = BuildType(configuration, cacheKey);
                TypeCache[cacheKey] = type;
            }
        }

        return (NativeConfigurableBase)Activator.CreateInstance(type)!;
    }

    private static Type BuildType(NativeConfigSchemaConfiguration configuration, string cacheKey)
    {
        var module = GetModule();
        var typeBuilder = module.DefineType($"NativeModConfig_{Interlocked.Increment(ref _typeCounter)}_{MakeIdentifier(cacheKey)}", TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.Sealed, typeof(NativeConfigurableBase));

        // Build the enums first, so they can be used as property types.
        // They are named after the config type, so two mods declaring the same enum name won't clash.
        var enums = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
        var displayCtor = GetCtor(typeof(DataAnnotations.DisplayAttribute), 0);
        var displayNameProperty = typeof(DataAnnotations.DisplayAttribute).GetProperty(nameof(DataAnnotations.DisplayAttribute.Name))!;
        foreach (var schemaEnum in CollectEnums(configuration))
        {
            var enumBuilder = module.DefineEnum($"{typeBuilder.FullName}.{MakeIdentifier(schemaEnum.Name)}", TypeAttributes.Public, typeof(int));
            for (int x = 0; x < schemaEnum.Members.Count; x++)
            {
                var member = schemaEnum.Members[x];
                var literal = enumBuilder.DefineLiteral(MakeIdentifier(member.Name), x);
                if (member.DisplayName != null)
                    literal.SetCustomAttribute(new CustomAttributeBuilder(displayCtor, Array.Empty<object>(), new[] { displayNameProperty }, new object[] { member.DisplayName }));
            }

            enums[schemaEnum.Name] = enumBuilder.CreateType();
        }

        // Build the properties.
        var ctor = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, Type.EmptyTypes);
        var ctorIl = ctor.GetILGenerator();
        var baseCtor = typeof(NativeConfigurableBase).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, binder: null, Type.EmptyTypes, modifiers: null)!;
        ctorIl.Emit(OpCodes.Ldarg_0);
        ctorIl.Emit(OpCodes.Call, baseCtor);

        foreach (var property in configuration.Properties)
        {
            var (propertyType, defaultValue) = ResolveTypeAndDefault(property, enums);
            var field = typeBuilder.DefineField($"_{MakeIdentifier(property.Name)}", propertyType, FieldAttributes.Private);
            EmitFieldInit(ctorIl, field, propertyType, defaultValue);

            var propertyBuilder = typeBuilder.DefineProperty(MakeIdentifier(property.Name), PropertyAttributes.None, propertyType, null);
            var getter = typeBuilder.DefineMethod($"get_{MakeIdentifier(property.Name)}", MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig, propertyType, Type.EmptyTypes);
            var getterIl = getter.GetILGenerator();
            getterIl.Emit(OpCodes.Ldarg_0);
            getterIl.Emit(OpCodes.Ldfld, field);
            getterIl.Emit(OpCodes.Ret);
            propertyBuilder.SetGetMethod(getter);

            var setter = typeBuilder.DefineMethod($"set_{MakeIdentifier(property.Name)}", MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig, null, new[] { propertyType });
            var setterIl = setter.GetILGenerator();
            setterIl.Emit(OpCodes.Ldarg_0);
            setterIl.Emit(OpCodes.Ldarg_1);
            setterIl.Emit(OpCodes.Stfld, field);
            setterIl.Emit(OpCodes.Ret);
            propertyBuilder.SetSetMethod(setter);

            foreach (var attribute in BuildAttributes(property, propertyType, defaultValue))
                propertyBuilder.SetCustomAttribute(attribute);
        }

        ctorIl.Emit(OpCodes.Ret);
        return typeBuilder.CreateType()!;
    }

    /// <summary>
    /// Declared enums plus one per property with inline <c>Values</c>.
    /// </summary>
    private static IEnumerable<NativeConfigSchemaEnum> CollectEnums(NativeConfigSchemaConfiguration configuration)
    {
        foreach (var schemaEnum in configuration.Enums)
            yield return schemaEnum;

        foreach (var property in configuration.Properties)
        {
            if (property.Values.Count > 0)
                yield return new NativeConfigSchemaEnum() { Name = property.Name, Members = property.Values };
        }
    }

    private static (Type propertyType, object? defaultValue) ResolveTypeAndDefault(NativeConfigSchemaProperty property, Dictionary<string, Type> enums)
    {
        switch (property.Type)
        {
            case NativeConfigSchemaProperty.SupportedTypes.Bool:
                return (typeof(bool), property.DefaultValue is bool b ? b : false);

            case NativeConfigSchemaProperty.SupportedTypes.Int:
                return (typeof(int), property.DefaultValue == null ? 0 : Convert.ToInt32(property.DefaultValue));

            case NativeConfigSchemaProperty.SupportedTypes.Float:
                return (typeof(float), property.DefaultValue == null ? 0.0f : Convert.ToSingle(property.DefaultValue));

            case NativeConfigSchemaProperty.SupportedTypes.Double:
                return (typeof(double), property.DefaultValue == null ? 0.0 : Convert.ToDouble(property.DefaultValue));

            case NativeConfigSchemaProperty.SupportedTypes.String:
                return (typeof(string), property.DefaultValue?.ToString());

            default:
                if (!enums.TryGetValue(property.Type, out var enumType))
                {
                    var hint = string.Equals(property.Type, "enum", StringComparison.OrdinalIgnoreCase)
                        ? $"Inline enums need a '{Keys.Values}' array on the property."
                        : $"Declare an enum with this name under '{Keys.Enums}'.";

                    throw new InvalidOperationException($"Property '{property.Name}' has unknown Type '{property.Type}'. {hint}");
                }

                return (enumType, GetEnumDefault(property, enumType));
        }
    }

    private static object GetEnumDefault(NativeConfigSchemaProperty property, Type enumType)
    {
        if (property.DefaultValue is string memberName)
        {
            var names = Enum.GetNames(enumType);
            for (int x = 0; x < names.Length; x++)
            {
                if (string.Equals(names[x], memberName, StringComparison.OrdinalIgnoreCase))
                    return Enum.ToObject(enumType, x);
            }

            throw new InvalidOperationException($"Property '{property.Name}' has DefaultValue '{memberName}' which is not a member of enum '{property.Type}'.");
        }

        return Enum.ToObject(enumType, 0);
    }

    private static void EmitFieldInit(ILGenerator il, FieldBuilder field, Type propertyType, object? defaultValue)
    {
        il.Emit(OpCodes.Ldarg_0);
        if (propertyType == typeof(string))
        {
            if (defaultValue == null)
                il.Emit(OpCodes.Ldnull);
            else
                il.Emit(OpCodes.Ldstr, (string)defaultValue);
        }
        else if (propertyType.IsEnum)
        {
            il.Emit(OpCodes.Ldc_I4, (int)defaultValue!);
        }
        else if (propertyType == typeof(bool))
        {
            il.Emit((bool)defaultValue! ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
        }
        else if (propertyType == typeof(int))
        {
            il.Emit(OpCodes.Ldc_I4, (int)defaultValue!);
        }
        else if (propertyType == typeof(float))
        {
            il.Emit(OpCodes.Ldc_R4, (float)defaultValue!);
        }
        else if (propertyType == typeof(double))
        {
            il.Emit(OpCodes.Ldc_R8, (double)defaultValue!);
        }
        else
        {
            il.Emit(OpCodes.Ldnull);
        }

        il.Emit(OpCodes.Stfld, field);
    }

    private static IEnumerable<CustomAttributeBuilder> BuildAttributes(NativeConfigSchemaProperty property, Type propertyType, object? defaultValue)
    {
        if (property.DisplayName != null)
            yield return new CustomAttributeBuilder(GetCtor(typeof(DisplayNameAttribute), 1), new object[] { property.DisplayName });

        if (property.Description != null)
            yield return new CustomAttributeBuilder(GetCtor(typeof(DescriptionAttribute), 1), new object[] { property.Description });

        if (property.Category != null)
            yield return new CustomAttributeBuilder(GetCtor(typeof(CategoryAttribute), 1), new object[] { property.Category });

        if (property.Order != null)
        {
            var orderProperty = typeof(DataAnnotations.DisplayAttribute).GetProperty(nameof(DataAnnotations.DisplayAttribute.Order))!;
            yield return new CustomAttributeBuilder(GetCtor(typeof(DataAnnotations.DisplayAttribute), 0), Array.Empty<object>(), new[] { orderProperty }, new object[] { property.Order.Value });
        }

        // The default value backs the Reset button of the configuration dialog.

        // Enums are skipped
        if (!propertyType.IsEnum)
        {
            var boxedDefault = defaultValue == null && propertyType == typeof(string) ? "" : defaultValue;
            var defaultValueCtor = typeof(DefaultValueAttribute).GetConstructor(new[] { typeof(object) })!;
            yield return new CustomAttributeBuilder(defaultValueCtor, new[] { boxedDefault! });
        }

        if (property.Slider != null)
        {
            var slider = property.Slider;
            if (propertyType != typeof(int) && propertyType != typeof(float) && propertyType != typeof(double))
                throw new InvalidOperationException($"Property '{property.Name}': sliders are only supported for int, float and double properties.");

            var tickPlacement = Enum.TryParse<SliderControlTickPlacement>(slider.TickPlacement, true, out var placement) ? placement : SliderControlTickPlacement.None;
            yield return new CustomAttributeBuilder(GetCtor(typeof(SliderControlParamsAttribute), 12), new object[]
            {
                slider.Minimum, slider.Maximum, slider.SmallChange, slider.LargeChange,
                slider.TickFrequency, slider.IsSnapToTickEnabled, tickPlacement,
                slider.ShowTextField, slider.IsTextFieldEditable, slider.TextValidationRegex,
                slider.TextFieldFormat, slider.TickFrequencyDouble
            });
        }

        if (property.FilePicker != null)
        {
            var file = property.FilePicker;
            if (propertyType != typeof(string))
                throw new InvalidOperationException($"Property '{property.Name}': file pickers are only supported for string properties.");

            yield return new CustomAttributeBuilder(GetCtor(typeof(FilePickerParamsAttribute), 13), new object[]
            {
                file.InitialDirectory!, (System.Environment.SpecialFolder)file.InitialFolderPath,
                file.ChooseFileButtonLabel, file.UserCanEditPathText, file.Title, file.Filter,
                file.FilterIndex, file.Multiselect, file.SupportMultiDottedExtensions,
                file.ShowHiddenFiles, file.ShowPreview, file.RestoreDirectory, file.AddToRecent
            });
        }

        if (property.FolderPicker != null)
        {
            var folder = property.FolderPicker;
            if (propertyType != typeof(string))
                throw new InvalidOperationException($"Property '{property.Name}': folder pickers are only supported for string properties.");

            yield return new CustomAttributeBuilder(GetCtor(typeof(FolderPickerParamsAttribute), 9), new object[]
            {
                folder.InitialDirectory!, (System.Environment.SpecialFolder)folder.InitialFolderPath,
                folder.ChooseFolderButtonLabel, folder.UserCanEditPathText, folder.Title,
                folder.OkButtonLabel, folder.FileNameLabel, folder.Multiselect, folder.ForceFileSystem
            });
        }
    }

    private static ModuleBuilder GetModule()
    {
        if (_module != null)
            return _module;

        var assembly = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("Reloaded.NativeModConfig.Dynamic"), AssemblyBuilderAccess.Run);
        _module = assembly.DefineDynamicModule("Main");
        return _module;
    }

    private static ConstructorInfo GetCtor(Type type, int parameterCount)
    {
        var constructor = type.GetConstructors().FirstOrDefault(c => c.GetParameters().Length == parameterCount);
        if (constructor == null)
            throw new InvalidOperationException($"No constructor with {parameterCount} parameters found on '{type.Name}'.");

        return constructor;
    }

    /// <summary>
    /// Makes a schema supplied name safe for use as a .NET identifier.
    /// </summary>
    private static string MakeIdentifier(string name)
    {
        var builder = new StringBuilder(name.Length);
        foreach (var character in name)
        {
            if (char.IsLetterOrDigit(character) || character == '_')
                builder.Append(character);
            else
                builder.Append('_');
        }

        if (builder.Length <= 0 || char.IsDigit(builder[0]))
            builder.Insert(0, '_');

        return builder.ToString();
    }
}
