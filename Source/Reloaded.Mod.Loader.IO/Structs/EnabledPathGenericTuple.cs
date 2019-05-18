namespace Reloaded.Mod.Loader.IO.Structs
{
    /// <summary>
    /// Stores a path, item to the path and whether the item is disabled.
    /// </summary>
    public class EnabledPathGenericTuple<TGeneric> : PathGenericTuple<TGeneric>
    {
        public bool IsEnabled { get; set; }

        /* Constructor/Destructor */

        public EnabledPathGenericTuple(string path, TGeneric o, bool isEnabled) : base(path, o)
        {
            IsEnabled = isEnabled;
        }

        public EnabledPathGenericTuple(string path, TGeneric o) : base(path, o)  { }
    }
}
