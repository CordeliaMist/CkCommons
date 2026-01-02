namespace CkCommons.DrawSystem;

// ================ NODE INTERFACES =================
// - Public Interfaces that help structure the framework of DynamicDrawSystem nodes.
// - Nodes themselves are public, but with internal setters to ensure integrity.

/// <summary>
///     Public accessor for a folder collection inside a DynamicDrawSystem. <para />
///     Children can be either <see cref="IDynamicFolderGroup{T}"/>s or <see cref="IDynamicFolder{T}"/>s.
/// </summary>
/// <remarks> A FolderCollection can be the Root folder if <see cref="IDynamicNode.Name"/> is string.Empty </remarks>
public interface IDynamicFolderGroup<T> : IDynamicCollection<T> where T : class
{
    /// <summary>
    ///     The FontAwesomeIcon 5 icon associated with this folder when opened, because why not.
    /// </summary>
    public FAI IconOpen { get; }

    /// <summary>
    ///     The FolderCollections and Folders contained by this collection, exposed for read-only access.
    /// </summary>
    public IReadOnlyList<IDynamicCollection<T>> Children { get; }

    /// <summary>
    ///     The instructions to be processed by any drawer wishing 
    ///     to display the folder in a sorted manner.
    /// </summary>
    public IReadOnlyDynamicSorter<IDynamicCollection<T>> Sorter { get; }
}

/// <summary>
///     Public accessor for a folder inside a DynamicDrawSystem. <para />
///     Children are always <see cref="IDynamicLeaf{T}"/>s.
/// </summary>
public interface IDynamicFolder<T> : IDynamicCollection<T> where T : class
{
    /// <summary>
    ///     The Leaves contained by this folder, exposed for read-only access.
    /// </summary>
    public IReadOnlyList<DynamicLeaf<T>> Children { get; }

    /// <summary>
    ///     The instructions to be processed by any drawer wishing 
    ///     to display the folder in a sorted manner.
    ///     (Maybe change this to let both FolderCollections and Folders share a common ISortMethod?)
    /// </summary>
    public IReadOnlyDynamicSorter<DynamicLeaf<T>> Sorter { get; }
}

/// <summary>
///     Public accessor for a folder node inside a DynamicDrawSystem. <para />
///     The Parent of a folder node must be a <see cref="DynamicFolderGroup{T}"/>.
/// </summary>
public interface IDynamicCollection<T> : IDynamicNode<T> where T : class
{
    /// <summary>
    ///     The separator used for calculating full paths.
    /// </summary>
    public string StringSplitter { get; }

    /// <summary>
    ///     The parent folder of this folder.
    /// </summary>
    public DynamicFolderGroup<T> Parent { get; }

    /// <summary>
    ///     Unique ID for a node in the DynamicDrawSystem. 
    ///     Only really implemented for Folders, see how we can define uniqueness for leaves.
    /// </summary>
    public uint ID { get; }

    /// <summary>
    ///     Associated Flags.
    /// </summary>
    public FolderFlags Flags { get; }

    /// <summary>
    ///     The color on the folder label when drawn.
    /// </summary>
    public uint NameColor { get; }

    /// <summary>
    ///     The FontAwesomeIcon 5 icon associated with this folder.
    /// </summary>
    public FAI Icon { get; }

    /// <summary>
    ///     The color of the icon when drawn.
    /// </summary>
    public uint IconColor { get; }

    /// <summary>
    ///     The background color of the folder when drawn.
    /// </summary>
    public uint BgColor { get; }

    /// <summary>
    ///     The border color of the folder when drawn.
    /// </summary>
    public uint BorderColor { get; }

    /// <summary>
    ///     The top gradient color used in the background fade effect behind the children when expanded.
    /// </summary>
    public uint GradientColor { get; }

    /// <summary>
    ///     How many child nodes are contained within the folder. (Does not include nested folder's children)
    /// </summary>
    public int TotalChildren { get; }

    /// <summary>
    ///     If this folder is the root folder. (Root has ID 0)
    /// </summary>
    public bool IsRoot { get; }

    /// <summary>
    ///     If the folder is expanded.
    /// </summary>
    public bool IsOpen { get; }

    /// <summary>
    ///     If this folder should render even when 0 children are present. (A helper for drawers)
    /// </summary>
    public bool ShowIfEmpty { get; }

    internal static bool Concat(IDynamicCollection<T> path, StringBuilder sb)
    {
        if (path.IsRoot)
            return false;

        if (Concat(path.Parent, sb))
            sb.Append(path.StringSplitter);
        sb.Append(path.Name);
        return true;
    }
}

/// <summary>
///     Public accessor for a leaf inside a DynamicDrawSystem. <para />
///     A Leaf can only exist as a child of a <see cref="IDynamicFolder{T}"/>.
/// </summary>
/// <typeparam name="T"> The data contained within this leaf. </typeparam>
public interface IDynamicLeaf<T> : IDynamicNode<T> where T : class
{
    /// <summary>
    ///     The parent folder of this leaf.
    /// </summary>
    public DynamicFolder<T> Parent { get; }

    /// <summary>
    ///     The data associated with this leaf.
    /// </summary>
    public T Data { get; }
}

/// <summary>
///     Dynamic Node but with a getter for ancestors. Helps keep search nodes free of generics.
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IDynamicNode<T> : IDynamicNode where T : class
{
    /// <summary>
    ///     Retrieve all ancestors of this node, excluding root.
    /// </summary>
    public IReadOnlyList<IDynamicCollection<T>> GetAncestors();
}

/// <summary>
///     Public accessor for a node inside a DynamicDrawSystem.
/// </summary>
public interface IDynamicNode
{
    /// <summary>
    ///     Precedence this node has if being filtered by folder groups or other node hierarchies.
    /// </summary>
    public int Priority { get; }

    /// <summary>
    ///     The Label associated with this node.
    /// </summary>
    public string Name { get; }

    /// <summary>
    ///     Path used by the DynamicDrawSystem for lookup and creation. <para />
    ///     Paths are generated by the hierarchy of node names. <para />
    ///     <b>FolderCollections</b> split paths with '//', while <b>Folders</b> use '/'.
    /// </summary>
    public string FullPath { get; }
}