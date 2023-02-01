namespace Reloaded.Mod.Loader.Update.Utilities;

/// <summary>
/// Represents a span with list-like addition semantics.
/// </summary>
public ref struct SpanList<T>
{
    /// <summary>
    /// Returns the current SpanList in the form of a Span.
    /// </summary>
    public Span<T> AsSpan => _items.Slice(0, Length);

    /// <summary>
    /// Current index of items in the list.
    /// </summary>
    public int Length;

    /// <summary>
    /// The underlying Span.
    /// </summary>
    private Span<T> _items;

    /// <summary>
    /// Creates a list-like wrapper for spans.
    /// </summary>
    /// <param name="items">The span of items to hold the results.</param>
    public SpanList(Span<T> items) : this() => _items = items;

    /// <summary>
    /// Adds an item onto this span list.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
    public void Add(scoped in T item) => _items[Length++] = item;
}