namespace CkCommons.DrawSystem;

/// <summary>
///     A representation of data nodes within a DynamicDrawSystem. <para />
///     Because they are created at any point in time based off <see cref="DynamicFolder{T}"/>'s 'EnsureLeaves' method,
///     There is no distinct ID bound to a leaf. <para />
///     Instead, its purpose is to hold the reference to its parents, it's name, and its data.
/// </summary>
public class DynamicLeaf<T> : IDynamicLeaf<T> where T : class
{
    public int Priority => 2;

    public DynamicFolder<T> Parent { get; internal set; }
    public T      Data      { get; internal set; }
    public string Name      { get; private set; }
    public string FullPath  { get; internal set; } = string.Empty;

    // Only allow creation internally, ensuring integrity within the file system.
    public DynamicLeaf(DynamicFolder<T> parent, string name, T data)
    {
        Parent = parent;
        Data = data;
        Name = name.FixName();
        UpdateFullPath();
    }

    public IReadOnlyList<IDynamicCollection<T>> GetAncestors()
    {
        if (Parent is null || Parent.IsRoot)
            return Array.Empty<IDynamicCollection<T>>();

        var ancestors = new List<IDynamicCollection<T>>();
        for (IDynamicCollection<T> cur = Parent; !cur.IsRoot; cur = cur.Parent)
            ancestors.Add(cur);

        return ancestors;
    }

    internal void SetParent(DynamicFolder<T> parent)
    {
        Parent = parent;
        UpdateFullPath();
    }

    internal void SetName(string name, bool fix)
    {
        Name = fix ? name.FixName() : name;
        UpdateFullPath();
    }

    internal void UpdateFullPath()
    {
        FullPath = $"{Parent.FullPath}/{Name}";
    }

    public override string ToString()
        => Name;
}