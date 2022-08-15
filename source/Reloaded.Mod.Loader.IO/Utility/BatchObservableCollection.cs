namespace Reloaded.Mod.Loader.IO.Utility;

/// <summary>
/// An abstraction over <see cref="ObservableCollection{T}"/> that allows for batch add/remove of items.
/// </summary>
public class BatchObservableCollection<T> : ObservableCollection<T>
{
    /// <inheritdoc />
    public BatchObservableCollection() { }

    /// <inheritdoc />
    public BatchObservableCollection(IEnumerable<T> collection) : base(collection) { }

    /// <inheritdoc />
    public BatchObservableCollection(List<T> list) : base(list) { }

    /// <summary>
    /// Adds and removes multiple collection items, notifying.
    /// </summary>
    /// <param name="add">Items to add to the collection.</param>
    /// <param name="remove">Items to remove.</param>
    public void AddAndRemoveRange(IEnumerable<T> add, IEnumerable<T> remove)
    {
        var items = Items;
        foreach (var toAdd in add)
        {
            var index = items.Count;
            items.Add(toAdd);
            this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, toAdd, index));
        }

        foreach (var toRemove in remove)
        {
            int index = items.IndexOf(toRemove);
            if (index == -1)
                continue;

            var item = items[index];
            items.RemoveAt(index);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index));
        }

        this.OnPropertyChanged(new PropertyChangedEventArgs("Count"));
        this.OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
    }
}