using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Loader.IO.Interfaces;
using Reloaded.Mod.Loader.IO.Structs;
using Reloaded.Mod.Loader.IO.Structs.Dependencies;
using Reloaded.Mod.Loader.IO.Structs.Sorting;
using Reloaded.Mod.Loader.IO.Weaving;

namespace Reloaded.Mod.Loader.IO.Config
{
    public class ModConfig : ObservableObject, IModConfig, IConfig
    {
        private static readonly ConfigReader<ModConfig> _modConfigReader = new ConfigReader<ModConfig>();

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
        /// Finds all mods on the filesystem, parses them and returns a list of all mods.
        /// </summary>
        /// <param name="modDirectory">Directory containing all of the mods.</param>
        public static List<PathGenericTuple<ModConfig>> GetAllMods(string modDirectory = null)
        {
            if (modDirectory == null)
                modDirectory = LoaderConfigReader.ReadConfiguration().ModConfigDirectory;

            return _modConfigReader.ReadConfigurations(modDirectory, ConfigFileName);
        }

        /// <summary>
        /// Returns a list of all of present and missing dependencies for a set of configurations.
        /// </summary>
        /// <param name="configurations">The mod configurations.</param>
        /// <param name="allMods">(Optional) List of all available mods.</param>
        public static ModDependencySet GetDependencies(IEnumerable<ModConfig> configurations, IEnumerable<ModConfig> allMods = null)
        {
            ModConfig[] allModsArray = allMods == null  ? GetAllMods().Select(x => x.Object).ToArray()
                                                        : allMods.ToArray();
            var dependencySets = new List<ModDependencySet>();

            foreach (var config in configurations)
                dependencySets.Add(GetDependencies(config, allModsArray));
            
            return new ModDependencySet(dependencySets);
        }

        /// <summary>
        /// Returns a list of all of present and missing dependencies for a given configuration.
        /// </summary>
        /// <param name="config">The mod configuration.</param>
        /// <param name="allMods">(Optional) List of all available mods.</param>
        public static ModDependencySet GetDependencies(ModConfig config, IEnumerable<ModConfig> allMods = null)
        {
            if (allMods == null)
                allMods = GetAllMods().Select(x => x.Object);

            var dependencySet = new ModDependencySet();
            var allModsArray = allMods.ToArray();

            // Populate whole list of nodes (graph), connected up with dependencies as edges.
            var allModsNodes = new List<Node<ModConfig>>();
            foreach (var mod in allModsArray)
                allModsNodes.Add(new Node<ModConfig>(mod));

            foreach (var node in allModsNodes)
                node.Edges = allModsNodes.Where(x => node.Element.ModDependencies.Contains(x.Element.ModId)).ToList();

            // Find our mod configuration in set of nodes.
            var initialNode = new Node<ModConfig>(config);
            initialNode.Edges = allModsNodes.Where(x => initialNode.Element.ModDependencies.Contains(x.Element.ModId)).ToList();

            // Recursive resolution.
            GetDependenciesVisitNode(initialNode, allModsArray, dependencySet);

            return dependencySet;
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
                SortModsVisitNode(firstUnvisitedNode, sortedMods);
            }

            return sortedMods;
        }

        /// <summary>
        /// Recursive function which given an unsorted node and the list it's going to be stored in, arranges
        /// a set of mod configurations.
        /// </summary>
        private static void SortModsVisitNode(Node<ModConfig> node, List<ModConfig> sortedMods)
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
                SortModsVisitNode(dependency, sortedMods);

            node.Visited = Mark.Visited;
            sortedMods.Add(node.Element);
        }

        /// <summary>
        /// Returns a list of all Mod IDs that are not installed referenced by a mod config.
        /// </summary>
        /// <param name="node">The mod configuration for which to find missing dependencies.</param>
        /// <param name="allMods">Collection containing all of the mod configurations.</param>
        /// <param name="dependencySet">Accumulator for all missing dependencies.</param>
        private static void GetDependenciesVisitNode(Node<ModConfig> node, ModConfig[] allMods, ModDependencySet dependencySet)
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
                GetDependenciesVisitNode(dependency, allMods, dependencySet);

            // Do collect missing dependencies.
            foreach (string dependencyId in node.Element.ModDependencies)
            {
                var dependencyConfig = allMods.FirstOrDefault(x => x.ModId == dependencyId);
                if (dependencyConfig != null)
                    dependencySet.Configurations.Add(dependencyConfig);
                else
                    dependencySet.MissingConfigurations.Add(dependencyId);
            }

            // Set visited and return to next in stack.
            node.Visited = Mark.Visited;
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
