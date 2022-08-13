namespace Reloaded.Mod.Loader.IO.Utility;

public static class Collections
{
    /// <summary>
    /// Modifies a given <see cref="ObservableCollection{T}"/> to turn the collection of <paramref name="oldItems"/> to <paramref name="newItems"/>.
    /// </summary>
    public static void ModifyObservableCollection<TItemType>(ObservableCollection<TItemType> oldItems, IEnumerable<TItemType> newItems)
    {
        ModifyObservableCollection(oldItems, newItems, out _, out _);
    }

    /// <summary>
    /// Modifies a given <see cref="ObservableCollection{T}"/> to turn the collection of <paramref name="oldItems"/> to <paramref name="newItems"/>.
    /// </summary>
    public static void ModifyObservableCollection<TItemType>(ObservableCollection<TItemType> oldItems, IEnumerable<TItemType> newItems, out HashSet<TItemType> itemsAdded, out HashSet<TItemType> itemsRemoved)
    {
        // Hash all the items.
        itemsAdded = newItems.ToHashSet();
        itemsRemoved = oldItems.ToHashSet();

        // Make a copy of hashed items.
        var itemsAddedCopy = new HashSet<TItemType>(itemsAdded);
        var itemsRemovedCopy = new HashSet<TItemType>(itemsRemoved);

        // Remove sets from each other.
        itemsAdded.ExceptWith(itemsRemovedCopy); // Remove Old Items from New Items
        itemsRemoved.ExceptWith(itemsAddedCopy); // Remove New Items from Old Items

        // Modify list.
        foreach (var newItem in itemsAdded)
            oldItems.Add(newItem);

        foreach (var removedItem in itemsRemoved)
            oldItems.Remove(removedItem);
    }

    /// <summary>
    /// Modifies a given <see cref="ObservableCollection{T}"/> to turn the collection of <paramref name="oldItems"/> to <paramref name="newItems"/>.
    /// </summary>
    public static void ModifyObservableCollection<TItemType>(BatchObservableCollection<TItemType> oldItems, IEnumerable<TItemType> newItems, out HashSet<TItemType> itemsAdded, out HashSet<TItemType> itemsRemoved)
    {        
        // Hash all the items.
        itemsAdded = newItems.ToHashSet();
        itemsRemoved = oldItems.ToHashSet();

        // Make a copy of hashed items.
        var itemsAddedCopy = new HashSet<TItemType>(itemsAdded);
        var itemsRemovedCopy = new HashSet<TItemType>(itemsRemoved);

        // Remove sets from each other.
        itemsAdded.ExceptWith(itemsRemovedCopy); // Remove Old Items from New Items
        itemsRemoved.ExceptWith(itemsAddedCopy); // Remove New Items from Old Items

        // Modify list.
        oldItems.AddAndRemoveRange(itemsAdded, itemsRemoved);
    }
}