using Dalamud.Bindings.ImGui;
using OtterGui.Extensions;

namespace CkCommons.DrawSystem.Selector;

public enum SelectionChange
{
    Added,
    Removed,
    Cleared,
}

/// <summary>
///     A Manager for all selections in a <see cref="DynamicDrawer{T}"/>. <para />
///     Pulls info from <see cref="DynamicFilterCache{T}"/> and <see cref="DynamicDrawSystem{T}"/>
/// </summary>
public class DynamicSelections<T> : IDisposable where T : class
{
    public delegate void SelectionChangeDelegate(SelectionChange kind, IEnumerable<IDynamicNode<T>> affected);
    public event SelectionChangeDelegate? SelectionChanged;

    private readonly DynamicDrawSystem<T> _parent;
    private readonly DynamicFilterCache<T> _cache;

    // Quick-Lookup HashSet of all selected entities regardless of type.
    private HashSet<IDynamicNode<T>> _selected = [];
    
    // Filtered Selection Caches (can remove later if not needed)
    private List<DynamicFolderGroup<T>> _selectedFolderGroups = [];
    private List<DynamicFolder<T>>      _selectedFolders      = [];
    private List<IDynamicCollection<T>> _selectedFoldersAll   = [];
    private List<DynamicLeaf<T>>        _selectedLeaves       = [];

    // Track history of latest selection to know how to perform CTRL+SHIFT Multi-Selection Jumps.
    protected IDynamicNode<T>? _lastSelected;
    protected IDynamicNode<T>? _lastAnchor;

    // Manages all nodes hover state over a 2 setters per draw-frame outside updates.
    protected IDynamicNode? _hoveredNode = null; // From last frame.
    protected IDynamicNode? _newHoveredNode = null; // Tracked each frame.

    public DynamicSelections(DynamicDrawSystem<T> parent, DynamicFilterCache<T> cache)
    {
        _parent = parent;
        _cache = cache;

        // Monitor the changes from the parents events.
        _parent.DDSChanged += OnDrawSystemChange;
        _parent.CollectionUpdated += OnCollectionChange;
    }

    public void Dispose()
    {
        _parent.DDSChanged -= OnDrawSystemChange;
        _parent.CollectionUpdated -= OnCollectionChange;
    }

    // Public Accessor for selection.
    public IReadOnlyList<IDynamicCollection<T>> Collections => _selectedFoldersAll;
    public IReadOnlyList<DynamicFolder<T>> Folders => _selectedFolders;
    public IReadOnlyList<DynamicLeaf<T>> Leaves => _selectedLeaves;
    public IReadOnlySet<IDynamicNode<T>> Selected => _selected;
    public IDynamicNode<T>? LastSelected => _lastSelected;

    public DynamicLeaf<T>? SelectedLeaf
        => _selectedLeaves.Count is 1 ? _selectedLeaves[0] : null;

    // Obtain if a particular node is selected.
    public bool IsSelected(IDynamicNode<T> entity)
        => _selected.Contains(entity);

    /// <summary>
    ///     Perform a full clear of all selected items in the DDS.
    /// </summary>
    public void ClearSelected()
    {
        var cleared = _selected.ToList();
        _selected.Clear();
        _selectedFolderGroups.Clear();
        _selectedFolders.Clear();
        _selectedFoldersAll.Clear();
        _selectedLeaves.Clear();
        _lastSelected = null;
        _lastAnchor = null;
        // Notify listeners.
        SelectionChanged?.Invoke(SelectionChange.Cleared, cleared);
    }

    // Deselect the entity, and remove it from all tracked selection lists.
    public void Deselect(IDynamicNode<T> entity)
        => DeselectInternal(entity);

    /// <summary>
    ///     Selects an entity in the DDS. <para /> 
    ///     Supports CTRL / SHIFT multi-selection behavior and updates accordingly. <para />
    ///     <b>NOTICE:</b>
    ///     This is very much pulled directly from Sundouleia's prototype model. It could be very prone to errors.
    /// </summary>
    /// <param name="entity"> The entity being selected. </param>
    /// <param name="canAnchorSelect"> If we allow CTRL based multi-selection. </param>
    /// <param name="canRangeSelect"> If we allow SHIFT based range selection. </param>
    public void SelectItem(IDynamicNode<T> entity, bool canAnchorSelect, bool canRangeSelect)
    {
        bool ctrl = ImGui.GetIO().KeyCtrl;
        bool shift = ImGui.GetIO().KeyShift;

        // SHIFT Range select / deselect.
        //  - LastAnchor must be valid.
        //  - LastAnchor cannot be the same as the current entity.
        if (shift && _lastAnchor != null && canRangeSelect && _lastAnchor != entity)
        {
            var fromIdx = _cache.FlatList.IndexOf(_lastAnchor);
            var toIdx = _cache.FlatList.IndexOf(entity);

            int start = Math.Min(fromIdx, toIdx);
            int end = Math.Max(fromIdx, toIdx);
            Svc.Log.Debug($"Range Selecting from {start} to {end} (Anchor:{_lastAnchor.Name}, Current:{entity.Name})");
            Svc.Log.Debug($"FlatList Count: {_cache.FlatList.Count}");

            bool selecting = !_selected.Contains(entity);
            for (int i = start; i <= end; i++)
            {
                if (selecting)
                    AddToSelected(_cache.FlatList[i]);
                else
                    DeselectInternal(_cache.FlatList[i]);
            }
            // Update last interacted.
            _lastSelected = selecting ? entity : null;
            _lastAnchor = entity;
        }
        // Single-Selection toggle
        else if (ctrl && canAnchorSelect)
        {
            var wasSelected = _selected.Contains(entity);
            if (wasSelected)
                DeselectInternal(entity);
            else
                AddToSelected(entity);
            // Update interactions.
            _lastAnchor = entity;
            _lastSelected = wasSelected ? null : entity;
        }
        // Normal interaction should immediately clear all other selections.
        else
        {
            // we can single select if nothing else is selected.
            if (_selected.Count is 1 && _lastSelected == entity)
            {
                ClearSelected();
                _lastAnchor = null;
                _lastSelected = null;
            }
            else
            {
                ClearSelected();
                AddToSelected(entity);
                _lastAnchor = entity;
                _lastSelected = entity;
            }
        }
    }

    public void SelectSingle(IDynamicNode<T> entity, bool addToSelection)
    {
        if (!addToSelection)
            ClearSelected();

        AddToSelected(entity);
        _lastAnchor = entity;
        _lastSelected = entity;
    }

    private void AddToSelected(IDynamicNode<T> entity)
    {
        if (!_selected.Add(entity))
            return;

        if (entity is DynamicFolderGroup<T> fc)
        {
            _selectedFolderGroups.Add(fc);
            _selectedFoldersAll.Add(fc);
        }
        else if (entity is DynamicFolder<T> f)
        {
            _selectedFolders.Add(f);
            _selectedFoldersAll.Add(f);
        }
        else if (entity is DynamicLeaf<T> l)
        {
            _selectedLeaves.Add(l);
        }

        SelectionChanged?.Invoke(SelectionChange.Added, [entity]);
    }

    // If we end up ever needing this, we should change the selection event to not rapid fire.
    public void SelectMultiple(IEnumerable<IDynamicNode<T>> entities)
    {
        foreach (var entity in entities)
            AddToSelected(entity);
    }

    private void DeselectInternal(IDynamicNode<T> entity)
    {
        if (!_selected.Remove(entity))
            return;

        if (entity is DynamicFolderGroup<T> fc)
        {
            _selectedFolderGroups.Remove(fc);
            _selectedFoldersAll.Remove(fc);
        }
        else if (entity is DynamicFolder<T> f)
        {
            _selectedFolders.Remove(f);
            _selectedFoldersAll.Remove(f);
        }
        else if (entity is DynamicLeaf<T> l)
        {
            _selectedLeaves.Remove(l);
        }

        SelectionChanged?.Invoke(SelectionChange.Removed, [entity]);
    }

    private void DeselectInternal(IEnumerable<IDynamicNode<T>> entities)
    {
        _selected.RemoveWhere(entities.Contains);
        _selectedFolderGroups = _selectedFolderGroups.Except(entities.OfType<DynamicFolderGroup<T>>()).ToList();
        _selectedFolders = _selectedFolders.Except(entities.OfType<DynamicFolder<T>>()).ToList();
        _selectedFoldersAll = _selectedFoldersAll.Except(entities.OfType<IDynamicCollection<T>>()).ToList();
        _selectedLeaves = _selectedLeaves.Except(entities.OfType<DynamicLeaf<T>>()).ToList();
        // Inform listeners.
        SelectionChanged?.Invoke(SelectionChange.Removed, entities);
    }

    /// <summary>
    ///     For ensuring that selections are cached between 
    ///     reloads to restore selections.
    /// </summary>
    private void OnDrawSystemChange(DDSChange type, IDynamicNode<T> _, IDynamicCollection<T>? __, IDynamicCollection<T>? ___)
    {
        switch (type)
        {
            case DDSChange.FullReloadStarting:
                // Do stuff.
                break;

            case DDSChange.FullReloadFinished:
                // Do other stuff.
                break;
        }
    }

    /// <summary>
    ///     FolderUpdated events can output removed paths, useful for deslecting nodes no longer present. <para />
    /// </summary>
    private void OnCollectionChange(CollectionUpdate kind, IDynamicCollection<T> collection, IEnumerable<DynamicLeaf<T>>? removed)
    {
        if (kind is CollectionUpdate.FolderUpdated)
            DeselectInternal(removed ?? []);
    }
}
