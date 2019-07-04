using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Loader.IO.Interfaces;
using Reloaded.Mod.Loader.IO.Structs;
using Reloaded.Mod.Loader.IO.Structs.Sorting;
using Reloaded.Mod.Loader.IO.Weaving;

namespace Reloaded.Mod.Loader.IO.Config
{
    public class ModConfig : ObservableObject, IModConfig, IConfig
    {
        private static ConfigReader<ModConfig> _modConfigReader = new ConfigReader<ModConfig>();

        /// <summary>
        /// The name of the configuration file as stored on disk.
        /// </summary>
        public const string ConfigFileName = "ModConfig.json";
        public const string IconFileName = "Preview.png";

        public string ModId             { get; set; } = "reloaded.template.modconfig";
        public string ModName           { get; set; } = "Reloaded Mod Config Template";
        public string ModAuthor         { get; set; } = "Someone";
        public string ModVersion        { get; set; } = "1.0.0";
        public string ModDescription    { get; set; } = "Template for a Reloaded Mod Configuration";
        public string ModDll            { get; set; } = "";
        public string ModIcon           { get; set; } = "";
        public string[] ModDependencies { get; set; } = new string[0];
        public string[] SupportedAppId  { get; set; } = new string[0];

        /*
           ---------
           Utilities
           --------- 
        */

        /// <summary>
        /// Retrieves the path to the individual DLL for this mod.
        /// </summary>
        /// <param name="configPath">The full path to the <see cref="ConfigFileName"/> configuration file. (See <see cref="PathGenericTuple{TGeneric}"/>)</param>
        /// <param name="modConfig">The individual mod configuration.</param>
        public static string GetDllPath(string configPath, ModConfig modConfig)
        {
            string configDirectory = Path.GetDirectoryName(configPath);
            return Path.Combine(configDirectory, modConfig.ModDll);
        }

        /// <summary>
        /// Writes the configuration to a specified file path.
        /// </summary>
        public static void WriteConfiguration(string path, ModConfig config)
        {
            var _applicationConfigLoader = new ConfigReader<ModConfig>();
            _applicationConfigLoader.WriteConfiguration(path, config);
        }

        /// <summary>
        /// Finds all mods on the filesystem, parses them and returns a list of
        /// all mods.
        /// </summary>
        public static List<PathGenericTuple<ModConfig>> GetAllMods()
        {
            return _modConfigReader.ReadConfigurations(LoaderConfigReader.ReadConfiguration().ModConfigDirectory, ConfigFileName);
        }

        /// <summary>
        /// Sorts a list of mods, taking into account individual dependencies between mods.
        /// </summary>
        public static List<ModConfig> SortMods(IEnumerable<PathGenericTuple<ModConfig>> mods)
        {
            return SortMods(mods.Select(x => x.Object));
        }

        /// <summary>
        /// Sorts a list of mods, taking into account individual dependencies between mods.
        /// </summary>
        public static List<ModConfig> SortMods(IEnumerable<ModConfig> mods)
        {
            var sortedMods = new List<ModConfig>();

            // Populate list of all nodes (without dependencies)
            List<Node<ModConfig>> allNodes = new List<Node<ModConfig>>();
            foreach (var mod in mods)
                allNodes.Add(new Node<ModConfig>(mod));

            // Generate list of edges to other nodes this node is dependent on.
            foreach (var node in allNodes)
                node.Edges = allNodes.Where(x => node.Element.ModDependencies.Contains(x.Element.ModId)).ToList();

            // Perform a depth first search so long a node is unvisited.
            Node<ModConfig> firstUnvisitedNode;
            while ((firstUnvisitedNode = allNodes.FirstOrDefault(node => node.Visited == Mark.NotVisited)) != null)
            {
                VisitNode(firstUnvisitedNode, sortedMods);
            }

            return sortedMods;
        }

        /// <summary>
        /// Recursive function which given an unsorted node and the list it's going to be stored in, arranges
        /// a set of mod configurations.
        /// </summary>
        private static void VisitNode(Node<ModConfig> node, List<ModConfig> sortedMods)
        {
            // Already fully visited, already in list.
            if (node.Visited == Mark.Visited)
                return;

            // Disallow looping on itself.
            // Not a directed acyclic graph.
            if (node.Visited == Mark.Visiting)
                return;

            node.Visited = Mark.Visiting;

            // Visit all children, depth first.
            foreach (var dependency in node.Edges)
                VisitNode(dependency, sortedMods);

            node.Visited = Mark.Visited;
            sortedMods.Add(node.Element);
        }

        /*
           ---------
           Overrides
           ---------
        */

        /* Useful for debugging. */
        public override string ToString()
        {
            return $"ModId: {ModId}, ModName: {ModName}";
        }

        /*
           ------------------------
           Overrides: Autogenerated
           ------------------------
        */

        protected bool Equals(ModConfig other)
        {
            return string.Equals(ModId, other.ModId) && 
                   string.Equals(ModName, other.ModName) && 
                   string.Equals(ModAuthor, other.ModAuthor) && 
                   string.Equals(ModVersion, other.ModVersion) && 
                   string.Equals(ModDescription, other.ModDescription) && 
                   string.Equals(ModDll, other.ModDll) && 
                   Enumerable.SequenceEqual(ModDependencies, other.ModDependencies);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            if (obj.GetType() != this.GetType())
                return false;

            return Equals((ModConfig)obj);
        }

        [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (ModId != null ? ModId.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ModName != null ? ModName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ModAuthor != null ? ModAuthor.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ModVersion != null ? ModVersion.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ModDescription != null ? ModDescription.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ModDll != null ? ModDll.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ModDependencies != null ? ModDependencies.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}
