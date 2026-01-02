namespace CkCommons.DrawSystem;

public interface IDynamicSorter<TItem> where TItem : class
{
    /// <summary>
    ///     Adds a sort step to the sorter. Duplicate methods are voided.
    /// </summary>
    void Add(ISortMethod<TItem> step);

    /// <summary>
    ///     Appends a series of steps to the sorter.
    /// </summary>
    void AddRange(IEnumerable<ISortMethod<TItem>> steps);

    /// <summary>
    ///     Assigns a series of steps to the sorter, replacing any existing ones.
    /// </summary>
    void SetSteps(IEnumerable<ISortMethod<TItem>> steps);

    /// <summary>
    ///     Removes a sort step.
    /// </summary>
    void Remove(ISortMethod<TItem> step);
    
    /// <summary>
    ///     Clears all sort steps.
    /// </summary>
    void Clear();

    /// <summary>
    ///     Reorders a sort step from oldIndex to newIndex.
    /// </summary>
    void Move(int oldIndex, int newIndex);
}