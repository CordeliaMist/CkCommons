namespace CkCommons.DrawSystem;

public class DynamicSorter<T> : IDynamicSorter<T>, IReadOnlyDynamicSorter<T> where T : class
{
    private readonly List<ISortMethod<T>> _sortSteps = [];

    /// <summary>
    ///     Constructor for optional parameters, defaulting to nothing being assigned. <para />
    /// </summary>
    public DynamicSorter(IEnumerable<ISortMethod<T>>? steps = null)
    {
        if (steps is not null)
            _sortSteps.AddRange(steps);
    }
    // Satisfy ReadOnly
    public bool FirstDescending { get; set; } = false;
    public int Count => _sortSteps.Count;
    public ISortMethod<T> this[int index] => _sortSteps[index];
    public IReadOnlyList<ISortMethod<T>> Steps => _sortSteps;

    public IEnumerator<ISortMethod<T>> GetEnumerator()
        => _sortSteps.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator()
        => _sortSteps.GetEnumerator();

    // Satisfy IDynamicSorter
    public void Add(ISortMethod<T> step)
    {
        if (!_sortSteps.Contains(step))
            _sortSteps.Add(step);
    }

    public void AddRange(IEnumerable<ISortMethod<T>> steps)
    {
        foreach (var step in steps)
            Add(step);
    }

    public void SetSteps(IEnumerable<ISortMethod<T>> steps)
    {
        _sortSteps.Clear();
        _sortSteps.AddRange(steps);
    }

    public void Remove(ISortMethod<T> sortMethod)
        => _sortSteps.Remove(sortMethod);

    public void Clear() => _sortSteps.Clear();

    public void Move(int oldIndex, int newIndex)
    {
        if (oldIndex < 0 || oldIndex >= _sortSteps.Count) return;
        if (newIndex < 0 || newIndex >= _sortSteps.Count) return;

        var m = _sortSteps[oldIndex];
        _sortSteps.RemoveAt(oldIndex);
        _sortSteps.Insert(newIndex, m);
    }

    public IEnumerable<T> SortItems(IEnumerable<T> items)
    {
        if (_sortSteps.Count is 0)
            return items;

        IOrderedEnumerable<T>? ordered = null;

        for (int i = 0; i < _sortSteps.Count; i++)
        {
            var key = _sortSteps[i].KeySelector;

            if (ordered == null)
                ordered = FirstDescending ? items.OrderByDescending(key) : items.OrderBy(key);
            else
                ordered = ordered.ThenBy(key);
        }

        return ordered ?? items;
    }
}