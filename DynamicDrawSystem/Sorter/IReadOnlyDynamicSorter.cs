namespace CkCommons.DrawSystem;

// Read-Only access to the dynamic sorter, without mutable methods.
public interface IReadOnlyDynamicSorter<TItem> : IReadOnlyList<ISortMethod<TItem>> where TItem : class
{
    /// <summary>
    ///     If the first step is in descending order.
    /// </summary>
    public bool FirstDescending { get; }

    /// <summary>
    ///     The steps of the dynamic sorter.
    /// </summary>
    IReadOnlyList<ISortMethod<TItem>> Steps { get; }

    /// <summary>
    ///     Sorts all passed in items with the sorter's steps.
    ///     Items must match the defined type <typeparamref name="T"/>.
    /// </summary>
    /// <param name="items"> The items to sort. </param>
    /// <param name="fallback"> The fallback to use. </param>
    IEnumerable<TItem> SortItems(IEnumerable<TItem> items);
}