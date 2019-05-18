using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Reloaded.Mod.Loader.IO.Structs;
using Reloaded.Mod.Loader.IO.Utility;
// ReSharper disable PossibleMultipleEnumeration

namespace Reloaded.Mod.Loader.IO
{
    /// <summary>
    /// A utility class which can generate or filter a list of enabled items by their individual configuration path.
    /// </summary>
    /// <remarks>
    ///     Reloaded II decides whether an item is enabled by storing a list of config paths
    ///     of enabled items relative to the directory containing the items.
    /// </remarks>
    public class EnabledItemConfigFilter
    {
        /// <summary>
        /// Directory which houses folders containing all items to be enabled/disabled.
        /// This should be an absolute full path.
        /// </summary>
        public string ItemDirectory { get; set; }

        /// <summary>
        /// Creates an item filterer which allows for the writing arrays of enabled items to be stored
        /// in configuration files.
        /// </summary>
        /// <param name="itemDirectory">
        ///     Directory which houses folders containing all items to be enabled/disabled.
        ///     This should be an absolute full path.
        /// </param>
        public EnabledItemConfigFilter(string itemDirectory)
        {
            ItemDirectory = itemDirectory;
        }

        /// <summary>
        /// Returns a list of all items wrapped in a <see cref="EnabledPathGenericTuple{TGeneric}"/>, which stores the item itself,
        /// the path of the item and whether the item is enabled.
        /// </summary>
        /// <param name="items">The collection of items to wrap and mark as enabled/disabled.</param>
        /// <param name="enabledItems">
        ///     Contains a collection of items to set as enabled.
        ///     An item is set as enabled if the item path given by <see cref="items[x].Path"/> is contained in
        ///     <see cref="ItemDirectory"/> + <see cref="enabledItems"/> (any item in <see cref="enabledItems"/>).
        /// </param>
        public List<EnabledPathGenericTuple<TItem>> GetItems<TItem>(IEnumerable<PathGenericTuple<TItem>> items, IEnumerable<string> enabledItems)
        {
            var newItems = new List<EnabledPathGenericTuple<TItem>>(items.Count());
            var enabledItemsSet = BuildEnabledSet(enabledItems);

            foreach (var item in items)
            {
                string itemPath = item.Path;
                bool isEnabled = IsItemEnabled(itemPath, enabledItemsSet);
                newItems.Add(new EnabledPathGenericTuple<TItem>(itemPath, item.Object, isEnabled));
            }

            return newItems;
        }

        /// <summary>
        /// Returns a list of all items wrapped in a <see cref="EnabledPathGenericTuple{TGeneric}"/>, which stores the item itself,
        /// the path of the item and whether the item is enabled.
        /// </summary>
        /// <param name="items">The collection of items to wrap and mark as enabled/disabled.</param>
        /// <param name="enabledItems">
        ///     Contains a collection of items to set as enabled.
        ///     An item is set as enabled if the item path given by <see cref="getItemPathFunction"/> is contained in
        ///     <see cref="ItemDirectory"/> + <see cref="enabledItems"/> (any item in <see cref="enabledItems"/>).
        /// </param>
        /// <param name="getItemPathFunction">A function which given an item, gets the absolute path of the item.</param>
        public List<EnabledPathGenericTuple<TItem>> GetItems<TItem>(IEnumerable<TItem> items, IEnumerable<string> enabledItems, Func<TItem, string> getItemPathFunction)
        {
            var newItems = new List<EnabledPathGenericTuple<TItem>>(items.Count());
            var enabledItemsSet = BuildEnabledSet(enabledItems);

            foreach (var item in items)
            {
                string itemPath = getItemPathFunction(item);
                bool isEnabled = IsItemEnabled(itemPath, enabledItemsSet);
                newItems.Add(new EnabledPathGenericTuple<TItem>(itemPath, item, isEnabled));
            }

            return newItems;
        }

        /// <summary>
        /// Checks whether an item is enabled by returning true if file exists.
        /// </summary>
        /// <param name="itemPath">Full path of item. (Should be relative to <see cref="ItemDirectory"/>)</param>
        /// <param name="enabledItems">
        ///     Contains a collection of items to set as enabled.
        ///     An item is set as enabled if the item path given by <see cref="itemPath"/> is contained in
        ///     <see cref="ItemDirectory"/> + <see cref="enabledItems"/> (any item in <see cref="enabledItems"/>).
        /// </param>
        public bool IsItemEnabled(string itemPath, IEnumerable<string> enabledItems)
        {
            // Build a set of items to 
            var hashSet = BuildEnabledSet(enabledItems);
            return IsItemEnabled(itemPath, hashSet);
        }

        /// <summary>
        /// Checks whether an item is enabled by returning true if file exists.
        /// </summary>
        /// <param name="itemPath">Full path of item. (Should be relative to <see cref="ItemDirectory"/>)</param>
        /// <param name="enabledItems">
        ///     Contains a collection of items to set as enabled.
        ///     An item is set as enabled if the item path given by <see cref="itemPath"/> is contained in
        ///     <see cref="ItemDirectory"/> + <see cref="enabledItems"/> (any item in <see cref="enabledItems"/>).
        /// </param>
        public bool IsItemEnabled(string itemPath, HashSet<string> enabledItems)
        {
            string fullItemPath = Path.GetFullPath(itemPath);
            if (enabledItems.Contains(fullItemPath))
                return true;

            return false;
        }

        /// <summary>
        /// Returns an array of relative paths of enabled items such that they can be
        /// stored in config files for later retrieval using <see cref="GetItems{TItem}(IEnumerable{Reloaded.Mod.Loader.IO.Structs.PathGenericTuple{TItem}})"/>.
        /// </summary>
        public List<string> GetRelativePaths<TItem>(IEnumerable<EnabledPathGenericTuple<TItem>> items)
        {
            var enabledModRelativePaths = new List<string>(items.Count());

            foreach (var item in items)
            {
                if (item.IsEnabled)
                {
                    string relativePath = RelativePaths.GetRelativePath(item.Path, ItemDirectory);
                    enabledModRelativePaths.Add(relativePath);
                }
            }

            return enabledModRelativePaths;
        }

        /// <summary>
        /// Builds a <see cref="HashSet{T}"/> of full paths of all enabled items for quick lookup.
        /// </summary>
        /// <param name="enabledItems">Set of enabled items (relative paths).</param>
        private HashSet<string> BuildEnabledSet(IEnumerable<string> enabledItems)
        {
            var hashSet = new HashSet<string>();

            foreach (var enabledItem in enabledItems)
            {
                // Remove leading slashes if present.
                string normalizedItemPath = enabledItem.TrimStart('\\');
                normalizedItemPath = normalizedItemPath.TrimStart('/');

                string fullPath = Path.Combine(ItemDirectory, normalizedItemPath);
                string normalizedFullPath = Path.GetFullPath(fullPath);
                hashSet.Add(normalizedFullPath);
            }

            return hashSet;
        }
    }
}
