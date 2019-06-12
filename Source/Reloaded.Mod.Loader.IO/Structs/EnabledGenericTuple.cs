namespace Reloaded.Mod.Loader.IO.Structs
{
    /// <summary>
    /// Stores a path, item to the path and whether the item is disabled.
    /// </summary>
    public class EnabledGenericTuple<TGeneric>
    {
        public bool IsEnabled { get; set; }
        public TGeneric Object { get; set; }

        /* Constructor/Destructor */

        public EnabledGenericTuple(TGeneric o, bool isEnabled)
        {
            IsEnabled = isEnabled;
            Object = o;
        }
    }
}
