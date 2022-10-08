namespace Reloaded.Mod.Launcher.Utility;

public static class CollectionExtensions
{
    private static Random _rng = new();

    /// <summary>
    /// Shuffles the order of items in this list.
    /// </summary>
    public static void Shuffle<T>(this IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = _rng.Next(n + 1);
            (list[k], list[n]) = (list[n], list[k]);
        }
    }
}
