namespace Reloaded.Mod.Loader.Update.Utilities.Nuget.Structs;

/// <summary>
/// Tuple that binds an item with a NuGet repository.
/// </summary>
public class NugetTuple<T> : Tuple<INugetRepository, T>
{
    /// <summary>
    /// The repository associated with this tuple.
    /// </summary>
    public INugetRepository Repository => Item1;
        
    /// <summary>
    /// Provides access to the generic element.
    /// </summary>
    public T Generic => Item2;

    /// <inheritdoc />
    public NugetTuple(INugetRepository item1, T item2) : base(item1, item2) { }
}