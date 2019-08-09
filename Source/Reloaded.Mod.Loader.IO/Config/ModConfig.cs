using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text.Json;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Loader.IO.Interfaces;
using Reloaded.Mod.Loader.IO.Misc;
using Reloaded.Mod.Loader.IO.Structs;
using Reloaded.Mod.Loader.IO.Structs.Dependencies;
using Reloaded.Mod.Loader.IO.Structs.Sorting;
using Reloaded.Mod.Loader.IO.Weaving;

namespace Reloaded.Mod.Loader.IO.Config
{
    public class ModConfig : ObservableObject, IModConfig, IConfig
    {
        /* Constants */
        public const string ConfigFileName  = "ModConfig.json";
        public const string IconFileName    = "Preview.png";

        /* Static defaults to prevent allocating new strings on object creation. */
        private const string DefaultId = "reloaded.template.modconfig";
        private const string DefaultName = "Reloaded Mod Config Template";
        private const string DefaultAuthor = "Someone";
        private const string DefaultVersion = "1.0.0";
        private const string DefaultDescription = "Template for a Reloaded Mod Configuration";

        private static readonly ConfigReader<ModConfig> _modConfigReader = new ConfigReader<ModConfig>();

        /* Class members. */
        public string ModId             { get; set; } = DefaultId;
        public string ModName           { get; set; } = DefaultName;
        public string ModAuthor         { get; set; } = DefaultAuthor;
        public string ModVersion        { get; set; } = DefaultVersion;
        public string ModDescription    { get; set; } = DefaultDescription;
        public string ModDll            { get; set; } = String.Empty;
        public string ModIcon           { get; set; } = String.Empty;

        public string[] ModDependencies         { get; set; }
        public string[] OptionalDependencies    { get; set; }
        public string[] SupportedAppId          { get; set; }

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
        /// <param name="modDirectory">(Optional) Directory containing all of the mods.</param>
        public static List<PathGenericTuple<ModConfig>> GetAllMods(string modDirectory = null)
        {
            if (modDirectory == null)
                modDirectory = LoaderConfigReader.ReadConfiguration().ModConfigDirectory;

            return _modConfigReader.ReadConfigurations(modDirectory, ConfigFileName);
        }

        /// <summary>
        /// Returns a list of all of present and missing dependencies for a set of configurations.
        /// Note: Does not include optional dependencies.
        /// </summary>
        /// <param name="configurations">The mod configurations.</param>
        /// <param name="allMods">(Optional) List of all available mods.</param>
        /// <param name="modDirectory">(Optional) Directory containing all of the mods.</param>
        [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
        public static ModDependencySet GetDependencies(IEnumerable<ModConfig> configurations, IEnumerable<ModConfig> allMods = null, string modDirectory = null)
        {
            var allModsCollection   = allMods ?? GetAllMods(modDirectory).Select(x => x.Object);
            var dependencySets      = new List<ModDependencySet>();

            foreach (var config in configurations)
                dependencySets.Add(GetDependencies(config, allModsCollection));
            
            return new ModDependencySet(dependencySets);
        }

        /// <summary>
        /// Returns a list of all of present and missing dependencies for a given configuration.
        /// Note: Does not include optional dependencies.
        /// </summary>
        /// <param name="config">The mod configuration.</param>
        /// <param name="allMods">(Optional) List of all available mods.</param>
        [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
        public static ModDependencySet GetDependencies(ModConfig config, IEnumerable<ModConfig> allMods = null)
        {
            if (allMods == null)
                allMods = GetAllMods().Select(x => x.Object);

            var dependencySet = new ModDependencySet();

            // Populate whole list of nodes (graph), connected up with dependencies as edges.
            var allModsNodes = new List<Node<ModConfig>>();
            foreach (var mod in allMods)
                allModsNodes.Add(new Node<ModConfig>(mod));

            foreach (var node in allModsNodes)
                node.Edges = allModsNodes.Where(x => node.Element.ModDependencies.Contains(x.Element.ModId)).ToList();

            // Find our mod configuration in set of nodes.
            var initialNode = new Node<ModConfig>(config);
            initialNode.Edges = allModsNodes.Where(x => initialNode.Element.ModDependencies.Contains(x.Element.ModId)).ToList();

            // Recursive resolution.
            GetDependenciesVisitNode(initialNode, allMods, dependencySet);

            return dependencySet;
        }

        /// <summary>
        /// Sorts a list of mods, taking into account individual dependencies between mods.
        /// Note: Does not include optional dependencies.
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
        private static void GetDependenciesVisitNode(Node<ModConfig> node, IEnumerable<ModConfig> allMods, ModDependencySet dependencySet)
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
           ---------------------------------
           Overrides: (Mostly) Autogenerated 
           ---------------------------------
        */

        protected bool Equals(ModConfig other)
        {
            return ModDependencies.SequenceEqualWithNullSupport(other.ModDependencies) &&
                   SupportedAppId.SequenceEqualWithNullSupport(other.SupportedAppId) &&
                   OptionalDependencies.SequenceEqualWithNullSupport(other.OptionalDependencies) &&
                   string.Equals(ModId, other.ModId) && 
                   string.Equals(ModName, other.ModName) && 
                   string.Equals(ModAuthor, other.ModAuthor) && 
                   string.Equals(ModVersion, other.ModVersion) && 
                   string.Equals(ModDescription, other.ModDescription) && 
                   string.Equals(ModDll, other.ModDll) && 
                   string.Equals(ModIcon, other.ModIcon);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ModConfig) obj);
        }

        public override int GetHashCode()
        {
            return (ModId != null ? ModId.GetHashCode() : 0);
        }
    }
}
