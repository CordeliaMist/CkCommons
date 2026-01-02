using Dalamud.Bindings.ImGui;

namespace CkCommons.DrawSystem.Selector;

// A DragDrop operation cache for the DynamicDrawSystem, keeping its filtered
// items to move in sync with the DynamicSelections, avoiding excessive allocations during dragging.
public class DynamicDragDrop<T> : IDisposable where T : class
{
    private readonly DynamicDrawer<T> _parent; // For getting the label, if desired.
    private readonly DynamicSelections<T> _selections;

    // The filtered nodes to move, containing nodes with no ancestors also being moved.
    private List<IDynamicNode<T>> _filteredToMove = [];

    public DynamicDragDrop(DynamicDrawer<T> parent, DynamicSelections<T> selections)
    {
        _parent = parent;
        _selections = selections;
        _selections.SelectionChanged += (_, __) => RefreshNodes();
    }

    public void Dispose()
    {
        _selections.SelectionChanged -= (_, __) => RefreshNodes();
    }

    // Can change overtime i guess.
    public string Label => $"{_parent.Label}Move";

    public string MoveString { get; private set; } = string.Empty;

    // Readonly access to entities
    public IReadOnlyList<IDynamicNode<T>> Nodes => _filteredToMove;

    public int Total => _filteredToMove.Count;
    public bool HasLeaves { get; private set; } = false;
    public bool OnlyCollections { get; private set; } = false;
    public bool OnlyFolderGroups { get; private set; } = false;
    public bool OnlyFolders { get; private set; } = false;
    public bool OnlyLeaves { get; private set; } = false;

    public bool IsActive => _filteredToMove.Count > 0;

    public bool IsValidTransfer(IDynamicNode<T> destNode)
        => Total > 0 && !(destNode is DynamicFolderGroup<T> && HasLeaves);

    private string GetMoveString()
    {
        if (_filteredToMove.Count is 0)
            return string.Empty;
        else if (_filteredToMove.Count == 1)
            return $"Moving {_filteredToMove.First().Name}...";
        else
        {
            var names = string.Join("\n\t - ", _filteredToMove.Select(e => e.Name));
            return $"Moving ...\n\t - {names}";
        }
    }

    // It is necessary to perform a full refresh, as the filtered nodes
    // could be outdated if their ancestors were selected or deselected
    // and we were unable to tell by the nodes passed in.
    public void RefreshNodes()
    {
        // Extract raw selection
        var selLeaves = _selections.Leaves;
        var selCollections = _selections.Collections;

        // Split into Folders and FolderGroups
        var selFolders = _selections.Collections.OfType<DynamicFolder<T>>().ToHashSet();
        var selGroups = _selections.Collections.OfType<DynamicFolderGroup<T>>().ToHashSet();
        
        // Remove any leaves whose parents are selected, as they will get transferred anyways.
        var filteredLeaves = selLeaves.Where(l => selFolders.Contains(l.Parent)).ToList();
        var filteredCollections = selCollections.Where(c => !c.GetAncestors().Any(selGroups.Contains)).ToList();

        // Update filtered.
        _filteredToMove = [ ..filteredLeaves, ..filteredCollections ];
        MoveString = GetMoveString();

        // Update flats.
        int leafCount = filteredLeaves.Count;
        int folderCount = filteredCollections.OfType<DynamicFolder<T>>().Count();
        int groupCount = filteredCollections.OfType<DynamicFolderGroup<T>>().Count();
        HasLeaves         = leafCount > 0;
        OnlyLeaves        = leafCount > 0  && folderCount == 0 && groupCount == 0;
        OnlyFolders       = folderCount > 0 && leafCount == 0  && groupCount == 0;
        OnlyFolderGroups  = groupCount > 0  && leafCount == 0  && folderCount == 0;
        OnlyCollections   = (folderCount + groupCount) > 0 && leafCount == 0;
    }
}
