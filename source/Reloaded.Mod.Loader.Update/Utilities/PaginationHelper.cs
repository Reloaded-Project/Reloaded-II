namespace Reloaded.Mod.Loader.Update.Utilities;

/// <summary>
/// Helper class that can be used to implement pagination in an application.
/// </summary>
public struct PaginationHelper : INotifyPropertyChanged
{
    /// <summary>
    /// Default pre-configured instance of the helper.
    /// </summary>
    public static readonly PaginationHelper Default = new ()
    {
        ItemsPerPage = 25
    };

    /// <summary>
    /// Number of items to skip when performing search query.
    /// </summary>
    public int Skip => Page * ItemsPerPage;

    /// <summary>
    /// Number of items to take when performing a search query.
    /// </summary>
    public int Take => ItemsPerPage;

    /// <summary>
    /// Number of items to display per page.
    /// </summary>
    public int ItemsPerPage { get; set; }

    /// <summary>
    /// The current application page.
    /// </summary>
    public int Page { get; private set; }
    
    /// <summary>
    /// Decrements the pagination helper to the previous page.
    /// </summary>
    public void PreviousPage(int numPages = 1)
    {
        Page -= numPages;
        if (Page < 0)
            Page = 0;
    }

    /// <summary>
    /// Advances the helper to the first page.
    /// </summary>
    public void Reset() => Page = 0;

    /// <summary>
    /// Advances the helper to the next page.
    /// </summary>
    public void NextPage(int numPages = 1)
    {
        if (Page != int.MaxValue)
            Page += numPages;
    }

    /// <summary>
    /// Returns a pagination helper incremented by given number of pages.
    /// </summary>
    public static PaginationHelper operator +(PaginationHelper a, int b)
    {
        a.NextPage(b);
        return a;
    }

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}