namespace CkCommons.DrawSystem.Selector;

/// <summary>
///     The Cache used by a <see cref="DynamicDrawer{T, TCache}"/>, which
///     holds information about the nodes visible for a searchable filter. <para />
///     Includes a lookup cachedNode map of all DrawSystem collections.
/// </summary>
/// <remarks> If you want to store additional data in the cache, inherit and add more data. </remarks>
public class DynamicFilterCache<T> : IDisposable where T : class
{
    private readonly DynamicDrawSystem<T> _parent;

    // Generic Utility
    private string _filter = string.Empty;
    
    // Cleanup Utility
    private bool                      _isDirty  = true;
    private HashSet<IDynamicCache<T>> _toReload = [];
    private HashSet<IDynamicCache<T>> _toSort   = [];

    // Mapping and Identification
    private List<IDynamicNode<T>>                               _flatNodeCache   = [];
    private Dictionary<IDynamicCollection<T>, IDynamicCache<T>> _cachedFolderMap = new();

    public DynamicFilterCache(DynamicDrawSystem<T> parent)
    {
        _parent = parent;
        RootCache = null!;
        // Monitor the changes from the parents events.
        _parent.DDSChanged += OnDrawSystemChange;
        _parent.CollectionUpdated += OnCollectionChange;
    }

    public void Dispose()
    {
        _parent.DDSChanged -= OnDrawSystemChange;
        _parent.CollectionUpdated -= OnCollectionChange;
    }

    /// <summary>
    ///     The Filter string used to generate the DynamicCache. <para />
    ///     Automatically marks the cache as dirty when updated.
    /// </summary>
    /// <remarks> _cacheDirty doesn't update if Filter is set to the same as current. </remarks>
    public string Filter
    {
        get => _filter;
        set
        {
            if (_filter != value)
            {
                _filter = value;
                _isDirty = true;
            }
        }
    }

    /// <summary>
    ///     The cached root node, with all sorting and filters applied. <para />
    ///     Used in most common shared draw functions that display in hierarchical form.
    /// </summary>
    /// <remarks> Use as public until we find a way to make a Read-Only version. </remarks>
    public DynamicFolderGroupCache<T> RootCache { get; private set; }

    /// <summary>
    ///     The flattened list of RootNodeCache. <para />
    ///     Used primarily for multi-selection, but not much elsewhere. <para />
    /// </summary>
    public IReadOnlyList<IDynamicNode<T>> FlatList => _flatNodeCache;

    /// <summary>
    ///     Maps all collections from the parent <see cref="DynamicDrawSystem{T}"/>'s Root Folder. <para />
    ///     Unlike RootNodeCache, this includes folders that were fully filtered out and have no children. <para />
    /// </summary>
    /// <remarks> Used when we are drawing individual folders, or a group of select folders that could have been filtered out. </remarks>
    public IReadOnlyDictionary<IDynamicCollection<T>, IDynamicCache<T>> CacheMap => _cachedFolderMap;

    /// <summary>
    ///     Generic call to mark the entire cache as dirty, requiring a full recalculation.
    /// </summary>
    public void MarkCacheDirty()
        => _isDirty = true;

    /// <summary>
    ///     Marks a single cached folder as dirty, requiring the cache update to
    ///     only recalculate one folder and its children over the entire cache.
    /// </summary>
    public void MarkForReload(IDynamicCollection<T> folder)
    {
        // Identify the cache for the folder via the map.
        if (_cachedFolderMap.TryGetValue(folder, out var cachedNode))
            _toReload.Add(cachedNode);
    }

    /// <summary>
    ///     Marks a sorter for a particular folder as dirty, 
    ///     forcing a re-sort of this folder's children only, saving on computation time.
    /// </summary>
    public void MarkForSortUpdate(IDynamicCollection<T> folder)
    {
        // Identify the cache for the folder via the map.
        // This will reference to the empty cache and/or the item in the root cache.
        if (_cachedFolderMap.TryGetValue(folder, out var cachedNode))
            _toSort.Add(cachedNode);
    }

    /// <summary>
    ///     Updates the current cache if the filter is marked as dirty, 
    ///     or handles per-folder recalclations where nessisary. <para />
    ///     <b> Fairly safe to call every drawframe. Only calculates on dirty filter. </b>
    /// </summary>
    public void UpdateCache()
    {
        if (_isDirty)
        {
            // Perform a full recalculation, nullifying all other changes.
            _toSort.Clear();
            _toReload.Clear();
            RootCache = new DynamicFolderGroupCache<T>(_parent.Root);
            BuildDynamicCache(RootCache);
            _flatNodeCache = [ RootCache.Folder, ..RootCache.GetAllDescendants() ];
            _isDirty = false;
            return;
        }

        // Otherwise, if we had any folders marked for reloading, process them recursively.
        if (_toReload.Count > 0)
        {
            // This will go through each folder and grab the filtered children again.
            // Also sorts the filtered result, and recursively calls BuildCachedFolder
            // on all sub-children.
            foreach (var cachedNode in _toReload.ToList())
                BuildDynamicCache(cachedNode);
            // Clear the nodes to reload.
            _toReload.Clear();
            // Update the flat cache.
            _flatNodeCache = [ RootCache.Folder, ..RootCache.GetAllDescendants() ];
        }

        // Finally, if we had any folders marked for re-sorting, process them non-recursively.
        if (_toSort.Count > 0)
        {
            foreach (var cachedNode in _toSort)
            {
                if (cachedNode is DynamicFolderGroupCache<T> fc)
                {
                    fc.Children = fc.Folder.Sorter
                        .SortItems(fc.Children.Select(c => c.Folder))
                        .Select(sorted => fc.Children.First(c => c.Folder == sorted)).ToList();
                }
                else if (cachedNode is DynamicFolderCache<T> f)
                {
                    f.Children = f.Folder.Sorter.SortItems(f.Folder.Children.Where(IsVisible)).ToList();
                }
            }
            // Clear the nodes to sort.
            _toSort.Clear();
            // Update the flat cache.
            _flatNodeCache = [ RootCache.Folder, ..RootCache.GetAllDescendants() ];
        }
    }

    /// <summary>
    ///     Recursively constructs a <paramref name="cachedNode"/>, filtering
    ///     out non-visible nodes, sorting remaining, and updating the map.
    /// </summary>
    private bool BuildDynamicCache(IDynamicCache<T> cachedNode)
    {
        // Assume visibility fails until proven otherwise.
        bool visible = false;

        // Add or Update to the map regardless of if it has children or not.
        _cachedFolderMap[cachedNode.Folder] = cachedNode;

        // If the cached group is a FolderCollection
        if (cachedNode is DynamicFolderGroupCache<T> fc)
        {
            // Firstly, get if the folder itself is visible.
            visible |= IsVisible(fc.Folder);
            // If the folder is opened, recursively process all children.
            if (fc.Folder.IsOpen)
            {
                // Pre-sort the children until we find a better solution for this.
                // Computation is minimal performance impact due to it only updating on dirty filters.
                // Assume we are going to be appending child nodes.
                var childNodes = new List<IDynamicCache<T>>();
                // Iterate through each one, recursively building the caches.
                foreach (var child in fc.Folder.Children)
                {
                    IDynamicCache<T> innerCache = child switch
                    {
                        IDynamicFolderGroup<T> c => new DynamicFolderGroupCache<T>(c),
                        IDynamicFolder<T> f => new DynamicFolderCache<T>(f),
                        _ => throw new InvalidOperationException("UNK CachedNodeType"),
                    };
                    // Build another CachedFolderNode for the child.
                    if (BuildDynamicCache(innerCache))
                    {
                        visible = true;
                        childNodes.Add(innerCache);
                    }
                }

                // Sorts the remaining child nodes by their folder's, and then selects the nodes for output.
                fc.Children = fc.Folder.Sorter
                    .SortItems(childNodes.Select(c => c.Folder))
                    .Select(sorted => childNodes.First(c => c.Folder == sorted)).ToList();
            }
            // Otherwise, if closed, scan to see if any sub-nodes are visible.
            else
            {
                // Scan to see if any sub-nodes are visible.
                visible |= IsCollapsedNodeVisible(fc.Folder);
            }
        }
        // If the cached group is a Folder
        else if (cachedNode is DynamicFolderCache<T> folder)
        {
            // Firstly, get if the folder itself is visible.
            visible |= IsVisible(folder.Folder);
            // If opened, recursively process all children.
            if (folder.Folder.IsOpen)
            {
                // Update the cached folder's children.
                folder.Children = folder.Folder.Sorter.SortItems(folder.Folder.Children.Where(IsVisible)).ToList();
                // If any children were added, the folder is visible.
                visible |= folder.Children.Any();
            }
            // Otherwise, if closed, scan to check for any possible visible leaves, setting them as single leaves if found.
            else
                visible |= IsCollapsedNodeVisible(folder.Folder);
        }

        // If after all of this, return if this node should be visible or not. If it shouldnt, we shouldnt append it.
        return visible;
    }

    /// <summary>
    ///     Scan for any descendants of a collapsed folder to see if any match. <para />
    ///     Check all sub-folders recursively, if ANY match, the nodes parent should be visible.
    /// </summary>
    protected bool IsCollapsedNodeVisible(IDynamicNode<T> node)
    {
        // If the folder itself is visible, display it.
        // Check if this entity itself matches visibility criteria.
        if (IsVisible(node))
            return true;

        // Otherwise, return true if ANY of the children inside of the closed folder are visible,
        if (node is DynamicFolderGroup<T> fc)
            return fc.Children.Any(IsCollapsedNodeVisible);
        else if (node is DynamicFolder<T> f)
            return f.Children.Any(IsCollapsedNodeVisible);

        // No visibility found at this node or deeper.
        return false;
    }

    /// <summary>
    ///     Used when obtaining the filtered results of an IDynamicNode. <para />
    ///     If you desire custom filter logic, override this in a parent class.
    /// </summary>
    protected virtual bool IsVisible(IDynamicNode<T> node)
        => Filter.Length is 0 || node.FullPath.Contains(Filter, StringComparison.OrdinalIgnoreCase);


    /// <summary>
    ///     Primarily to automatically update the cache whenever an 
    ///     internal change to the hierarchy occurs.
    /// </summary>
    private void OnDrawSystemChange(DDSChange type, IDynamicNode<T> obj, IDynamicCollection<T>? _, IDynamicCollection<T>? __)
    {
        switch (type)
        {
            case DDSChange.CollectionAdded:
            case DDSChange.CollectionRemoved:
            case DDSChange.CollectionMoved:
            case DDSChange.BulkMove:
            case DDSChange.CollectionMerged:
            case DDSChange.CollectionRenamed:
                // Mark the entire cache as dirty, because nested children
                // could be added and we dont have a way to update specific nodes yet.
                _isDirty = true;
                break;
        }
    }

    /// <summary>
    ///     Ensure only the parts of the cache that should be updated, are updated. <para />
    ///     Helps save on computation time for recalculations.
    /// </summary>
    private void OnCollectionChange(CollectionUpdate kind, IDynamicCollection<T> collection, IEnumerable<DynamicLeaf<T>>? _)
    {
        switch (kind)
        {
            case CollectionUpdate.FolderUpdated:
                MarkForReload(collection);
                break;
            case CollectionUpdate.SortDirectionChange:
            case CollectionUpdate.SorterChange:
                MarkForSortUpdate(collection);
                break;
            case CollectionUpdate.OpenStateChange:
                // Reload the parent, incase the folder is now to be shown.
                MarkForReload(collection.Parent);
                break;
        }
    }

}