namespace Reloaded.Mod.Launcher.Utility;

internal static class ListViewUtilities
{
    /// <summary>
    /// Moves an item displayed within the listview by specified
    /// number of places while retaining focus.
    /// </summary>
    /// <param name="listView">The listview for which to move the item for.</param>
    /// <param name="offset">The offset, -1, 0 or 1 to move the item by.</param>
    /// <returns></returns>
    internal static bool ShiftItem(this ListView listView, int offset)
    {
        if (listView.SelectedIndex == -1)
            return false;

        var index = listView.SelectedIndex;
        var list = TryGetUnderlyingList(listView);
        if (list == null)
            return false;

        var insertIndex = GetInsertIndex(index, offset, list.Count);
        if (insertIndex == -1)
            return false;

        var item = list[index]!;
        list!.RemoveAt(index);
        list.Insert(insertIndex, item);
        listView.SelectedIndex = insertIndex;
        listView.ScrollIntoView(item);
        return true;
    }

    private static IList? TryGetUnderlyingList(ListView listView)
    {
        var source = listView.ItemsSource;
        if (source is ICollectionView view)
            return view.SourceCollection as IList;

        throw new Exception($"The source: {source} is not supported.");
    }

    private static int GetInsertIndex(int index, int indexOffset, int totalItemCount)
    {
        return indexOffset switch
        {
            -1 when index > 0 => index - 1,
            1 when index < totalItemCount => index + 1,
            _ => -1
        };
    }
}