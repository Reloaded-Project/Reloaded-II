namespace Reloaded.Mod.Launcher.Lib.Models.Model.Update;

/// <summary>
/// Selectable mode for search sorting.
/// </summary>
public class SearchSortingMode : ObservableObject
{
    /// <summary>
    /// Friendly name for the sort.
    /// </summary>
    public string FriendlyName { get; private set; } = string.Empty;

    /// <summary>
    /// Sorting mode used in this search.
    /// </summary>
    public SearchSorting SortingMode { get; private set; }
    
    /// <summary>
    /// Whether this search is descending.
    /// </summary>
    public bool IsDescending { get; private set; }

    /// <summary>
    /// Gets all possible combinations.
    /// </summary>
    /// <returns></returns>
    public static SearchSortingMode[] GetAll()
    {
        var sorts = Enum.GetValues<SearchSorting>();
        var orderModes = new bool[] { true, false };

        var options = new SearchSortingMode[sorts.Length * orderModes.Length];
        var currentIndex = 0;

        foreach (var sort in sorts)
        foreach (var orderMode in orderModes) 
        {
            options[currentIndex++] = Get(sort, orderMode);
        }

        return options;
    }

    /// <summary>
    /// Gets a search sorting mode with the given sort and order.
    /// </summary>
    public static SearchSortingMode Get(SearchSorting sort, bool orderMode)
    {
        var option = new SearchSortingMode()
        {
            SortingMode = sort,
            IsDescending = orderMode
        };

        string sortString = sort switch
        {
            SearchSorting.None => "None",
            SearchSorting.LastModified => Resources.SearchOptionSortLastModified.Get(),
            SearchSorting.Downloads => Resources.SearchOptionSortDownloads.Get(),
            SearchSorting.Likes => Resources.SearchOptionSortLikes.Get(),
            SearchSorting.Views => Resources.SearchOptionSortViews.Get(),
            _ => Enum.GetName<SearchSorting>(sort) ?? "Unknown"
        };

        string sortOrder = orderMode 
            ? Resources.SearchOptionDescending.Get() 
            : Resources.SearchOptionAscending.Get();

        option.FriendlyName = string.Format("{0} ({1})", sortString, sortOrder);
        return option;
    }
}
