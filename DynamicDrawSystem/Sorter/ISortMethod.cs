namespace CkCommons.DrawSystem;

public interface ISortMethod<T> : IComparable<ISortMethod<T>> where T : class
{
    /// <summary>
    ///     The name of this sorter step. (Used for comparison and identification)
    /// </summary>
    string Name { get; }

    /// <summary>
    ///     Icon used for display, if any.
    /// </summary>
    FAI Icon { get; }

    /// <summary>
    ///     For the CkGui.AttachTooltip
    /// </summary>
    string Tooltip { get; }

    /// <summary>
    ///     Depicts what part of <typeparamref name="T"/> is used for sorting.
    /// </summary>
    Func<T, IComparable?> KeySelector { get; }

    /// <summary>
    ///     Compare to other sorter steps by name.
    /// </summary>
    int IComparable<ISortMethod<T>>.CompareTo(ISortMethod<T>? other)
        => string.Compare(Name, other?.Name, StringComparison.Ordinal);
}