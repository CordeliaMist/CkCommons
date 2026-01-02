namespace CkCommons.DrawSystem.Selector;

/// <summary>
///     Inherits <see cref="IDynamicCache{T}"/> 
///     for a standard <see cref="IDynamicFolder{T}"/>.
/// </summary>
public class DynamicFolderCache<T>(IDynamicFolder<T> folder) : IDynamicCache<T> where T : class
{
    public IDynamicFolder<T> Folder { get; } = folder;
    public IReadOnlyList<IDynamicLeaf<T>> Children { get; set; } = [];

    public bool IsEmpty
        => Folder is null;
    public IEnumerable<IDynamicNode<T>> GetAllDescendants()
        => Children;

    IDynamicCollection<T> IDynamicCache<T>.Folder => Folder;
}