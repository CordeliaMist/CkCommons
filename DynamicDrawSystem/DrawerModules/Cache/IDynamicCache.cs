namespace CkCommons.DrawSystem.Selector;

/// <summary>
///     A cached version of a <see cref="IDynamicCollection{T}"/>, with 
///     all children filtered and sorted via the folders <see cref="DynamicSorter{T}"/>.
/// </summary>
public interface IDynamicCache<T> where T : class
{
    IDynamicCollection<T> Folder { get; }
    public bool IsEmpty { get; }

    public IEnumerable<IDynamicNode<T>> GetAllDescendants();
}