using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Reloaded.Mod.Launcher.Utility
{
    public static class Collections
    {
        /// <summary>
        /// Updates a given <see cref="ObservableCollection{T}"/> to turn the collection of <see cref="oldItems"/> to <see cref="newItems"/>.
        /// If the old item collection is null, the new item collection is inserted.
        /// </summary>
        public static void UpdateObservableCollection<TItemType>(ref ObservableCollection<TItemType> oldItems, IEnumerable<TItemType> newItems)
        {
            if (oldItems != null)
            {
                ModifyObservableCollection(oldItems, newItems);
            }
            else
            {
                oldItems = new ObservableCollection<TItemType>(newItems);
            }
        }

        /// <summary>
        /// Modifies a given <see cref="ObservableCollection{T}"/> to turn the collection of <see cref="oldItems"/> to <see cref="newItems"/>.
        /// </summary>
        public static void ModifyObservableCollection<TItemType>(ObservableCollection<TItemType> oldItems, IEnumerable<TItemType> newItems)
        {
            // Add new entries.
            // In new set but not old set: loadedMods \ currentSet
            var newMods = newItems.ToHashSet();
            newMods.ExceptWith(oldItems.ToHashSet());

            // Remove old entries.
            var oldMods = oldItems.ToHashSet();
            oldMods.ExceptWith(newItems.ToHashSet());

            // Modify list.
            foreach (var newMod in newMods)
                oldItems.Add(newMod);

            foreach (var removedMod in oldMods)
                oldItems.Remove(removedMod);
        }
    }
}
