using Reloaded.Mod.Loader.IO.Config;

namespace Reloaded.Mod.Loader.IO.Structs
{
    /// <summary>
    /// A tuple class which stores a string and a config type.
    /// </summary>
    public class PathTuple<TGeneric> where TGeneric : IConfig<TGeneric>, new()
    {
        /// <summary>
        /// The file path to the object.
        /// </summary>
        public string Path      { get; set; }

        /// <summary>
        /// The object in question.
        /// </summary>
        public TGeneric Config  { get; set; }

        public PathTuple(string path, TGeneric o)
        {
            Path = path;
            Config = o;
        }

        /// <summary>
        /// Saves the current object to file.
        /// </summary>
        public void Save() => IConfig<TGeneric>.ToPath(Config, Path);
    }
}
