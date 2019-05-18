namespace Reloaded.Mod.Loader.IO.Structs
{
    /// <summary>
    /// A tuple class which stores a string and a generic type.
    /// </summary>
    public class PathGenericTuple<TGeneric>
    {
        public string Path      { get; set; }
        public TGeneric Object  { get; set; }

        public PathGenericTuple(string path, TGeneric o)
        {
            Path = path;
            Object = o;
        }
    }
}
