namespace CkCommons.DrawSystem.Selector;

/// <summary>
///     Inherits <see cref="IDynamicCache{T}"/> 
///     for a standard <see cref="IDynamicFolderGroup{T}"/>.
/// </summary>
public class DynamicFolderGroupCache<T>(IDynamicFolderGroup<T> folder) : IDynamicCache<T> where T : class
{
    public IDynamicFolderGroup<T> Folder { get; } = folder;
    public IReadOnlyList<IDynamicCache<T>> Children { get; set; } = [];

    public bool IsEmpty
        => Folder is null;
    public IEnumerable<IDynamicNode<T>> GetAllDescendants()
        => Children.SelectMany(c => c.GetAllDescendants().Prepend(c.Folder));

    IDynamicCollection<T> IDynamicCache<T>.Folder => Folder;
}