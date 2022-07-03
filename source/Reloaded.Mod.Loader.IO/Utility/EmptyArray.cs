namespace Reloaded.Mod.Loader.IO.Utility;

/// <summary>
/// Represents a non-modifiable empty array.
/// </summary>
public static class EmptyArray<T>
{
    public static readonly T[] Instance = new T[0];
}