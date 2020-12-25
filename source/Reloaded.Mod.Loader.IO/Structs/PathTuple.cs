namespace Reloaded.Mod.Loader.IO.Structs
{
    /// <summary>
    /// A tuple class which stores a string and a generic type.
    /// </summary>
    public class PathTuple<TGeneric>
    {
        /// <summary>
        /// The file path to the object.
        /// </summary>
        public string Path      { get; set; }

        /// <summary>
        /// The object in question.
        /// </summary>
        public TGeneric Object  { get; set; }

        public PathTuple(string path, TGeneric o)
        {
            Path = path;
            Object = o;
        }
    }
}
