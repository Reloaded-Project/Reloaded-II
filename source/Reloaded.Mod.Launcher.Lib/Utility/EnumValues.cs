namespace Reloaded.Mod.Launcher.Lib.Utility;

/// <summary>
/// Provides various values for an enum of type T.
/// </summary>
public static class EnumValues<T> where T : struct, Enum
{
    /// <summary>
    /// Maximum value of enum.
    /// </summary>
    public static readonly T Max;

    /// <summary>
    /// Minimum value of enum.
    /// </summary>
    public static readonly T Min;
    
    static EnumValues()
    {
        var values = Enum.GetValues<T>();
        Max = values.Max();
        Min = values.Min();
    }
}