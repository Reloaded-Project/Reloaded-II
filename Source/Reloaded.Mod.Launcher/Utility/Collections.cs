using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Reloaded.Mod.Launcher.Utility
{
    public static class Collections
    {
        /// <summary>
        /// Modifies a given <see cref="ObservableCollection{T}"/> to turn the collection of <see cref="oldItems"/> to <see cref="newItems"/>.
        /// </summary>
        public static void ModifyObservableCollection<TItemType>(ObservableCollection<TItemType> oldItems, IEnumerable<TItemType> newItems)
        {
            // Hash all the items.
            var newItemSet = newItems.ToHashSet();
            var oldItemSet = oldItems.ToHashSet();

            // Make a copy of hashed items.
            var newItemSetCopy = new HashSet<TItemType>(newItemSet);
            var oldItemSetCopy = new HashSet<TItemType>(oldItemSet);

            // Remove sets from each other.
            newItemSet.ExceptWith(oldItemSetCopy);
            oldItemSet.ExceptWith(newItemSetCopy);

            // Modify list.
            foreach (var newMod in newItemSet)
                oldItems.Add(newMod);

            foreach (var removedMod in oldItemSet)
                oldItems.Remove(removedMod);
        }
    }
}
