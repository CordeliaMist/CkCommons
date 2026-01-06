using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using OtterGui.Text;
using Serilog;

namespace CkCommons.DrawSystem.Selector;

// Initialization and base calls for the selector.
public partial class DynamicDrawer<T> : IDisposable 
    where T : class
{
    protected readonly ILogger Log;
    protected readonly DynamicDrawSystem<T>  DrawSystem;
    protected readonly DynamicFilterCache<T> FilterCache;
    protected readonly DynamicSelections<T>  Selector;
    protected readonly DynamicDragDrop<T>    DragDrop;

    // Queue of all actions to perform after completely drawing the list,
    // to avoid processing operations that would modify the filter mid-draw. (Can be reworked later if we need to)
    private readonly Queue<Action> _postDrawActions = new();

    public DynamicDrawer(string label, ILogger log, DynamicDrawSystem<T> drawSystem, 
        DynamicFilterCache<T>? cache = null)
    {
        Label = label;

        Log         = log;
        DrawSystem  = drawSystem;
        FilterCache = cache ?? new DynamicFilterCache<T>(drawSystem);
        Selector    = new(drawSystem, FilterCache);
        DragDrop    = new(this, Selector);
    }

    public string Label { get; init; }

    public virtual void Dispose()
    {
        Selector.Dispose();
        FilterCache.Dispose();
    }

    // Manages all nodes hover state over a 2 setters per draw-frame outside updates.
    protected IDynamicNode? _hoveredNode = null; // From last frame.
    protected IDynamicNode? _newHoveredNode = null; // Tracked each frame.

    /// <inheritdoc cref="DrawContents(float, float, float, DynamicFlags)"/>
    public void DrawContents(DynamicFlags flags = DynamicFlags.None)
        => DrawContents(ImGui.GetContentRegionAvail().X, flags);

    /// <inheritdoc cref="DrawContents(float, float, float, DynamicFlags)"/>
    public void DrawContents(float width, DynamicFlags flags = DynamicFlags.None)
        => DrawContents(width, ImUtf8.FrameHeight, ImUtf8.FrameHeight + ImUtf8.ItemInnerSpacing.X, flags);

    /// <inheritdoc cref="DrawContents(float, float, float, DynamicFlags)"/>
    public void DrawContents(float width, float indent, DynamicFlags flags = DynamicFlags.None)
        => DrawContents(width, indent, indent, flags);

    /// <summary>
    ///     Draws the filtered DynamicDrawer.
    /// </summary>
    /// <param name="width"> The width of the draw area. </param>
    /// <param name="groupIndent"> The indent to use for folder groups. </param>
    /// <param name="indent"> The indent to use for folders. </param>
    /// <param name="flags"> The flags to use for drawing. </param>
    public void DrawContents(float width, float groupIndent, float indent, DynamicFlags flags = DynamicFlags.None)
    {
        using var _ = ImRaii.Child(Label, new Vector2(width, -1), false, WFlags.NoScrollbar);
        if (!_) return;

        HandleMainContextActions();
        FilterCache.UpdateCache();

        // Set the style for the draw logic.
        ImGui.SetScrollX(0);
        using var s = ImRaii.PushStyle(ImGuiStyleVar.WindowPadding, Vector2.One)
             .Push(ImGuiStyleVar.IndentSpacing, 14f * ImGuiHelpers.GlobalScale);

        DrawFolderGroupChildren(FilterCache.RootCache, groupIndent, indent, flags);
        PostDraw();
    }

    /// <inheritdoc cref="DrawContents{TFolder}(float, float, float, DynamicFlags)"/>
    public void DrawContents<TFolder>(DynamicFlags flags = DynamicFlags.None) where TFolder : DynamicFolder<T>
        => DrawContents<TFolder>(ImGui.GetContentRegionAvail().X, flags);

    /// <inheritdoc cref="DrawContents{TFolder}(float, float, float, DynamicFlags)"/>
    public void DrawContents<TFolder>(float width, DynamicFlags flags = DynamicFlags.None) where TFolder : DynamicFolder<T>
        => DrawContents<TFolder>(width, ImUtf8.FrameHeight, ImUtf8.FrameHeight + ImUtf8.ItemInnerSpacing.X, flags);

    /// <inheritdoc cref="DrawContents{TFolder}(float, float, float, DynamicFlags)"/>
    public void DrawContents<TFolder>(float width, float indent, DynamicFlags flags = DynamicFlags.None) where TFolder : DynamicFolder<T>
        => DrawContents<TFolder>(width, indent, indent, flags);

    /// <summary>
    ///     Draws the filtered DynamicDrawer, showing only folders of type <typeparamref name="TFolder"/>.
    /// </summary>
    /// <param name="width"> The width of the draw area. </param>
    /// <param name="groupIndent"> The indent to use for folder groups. </param>
    /// <param name="indent"> The indent to use for folders. </param>
    /// <param name="flags"> The flags to use for drawing. </param>
    public void DrawContents<TFolder>(float width, float groupIndent, float indent, DynamicFlags flags = DynamicFlags.None)
        where TFolder : DynamicFolder<T>
    {
        using var _ = ImRaii.Child(Label, new Vector2(width, -1), false, WFlags.NoScrollbar);
        if (!_) return;

        HandleMainContextActions();
        FilterCache.UpdateCache();

        // Set the style for the draw logic.
        ImGui.SetScrollX(0);
        using var s = ImRaii.PushStyle(ImGuiStyleVar.WindowPadding, Vector2.One)
             .Push(ImGuiStyleVar.IndentSpacing, 14f * ImGuiHelpers.GlobalScale);

        DrawFolderGroupChildren<TFolder>(FilterCache.RootCache, groupIndent, indent, flags);
        PostDraw();
    }

    /// <inheritdoc cref="DrawFolder(IDynamicCollection{T}, float, float, float, DynamicFlags)"/>
    public void DrawFolder(IDynamicCollection<T> folder, DynamicFlags flags = DynamicFlags.None)
        => DrawFolder(folder, ImGui.GetContentRegionAvail().X, flags);

    /// <inheritdoc cref="DrawFolder(IDynamicCollection{T}, float, float, float, DynamicFlags)"/>
    public void DrawFolder(IDynamicCollection<T> folder, float width, DynamicFlags flags = DynamicFlags.None)
        => DrawFolder(folder, width, ImUtf8.FrameHeight, ImUtf8.FrameHeight + ImUtf8.ItemInnerSpacing.X, flags);

    /// <inheritdoc cref="DrawFolder(IDynamicCollection{T}, float, float, float, DynamicFlags)"/>
    public void DrawFolder(IDynamicCollection<T> folder, float width, float indent, DynamicFlags flags = DynamicFlags.None)
        => DrawFolder(folder, width, indent, indent, flags);

    /// <summary>
    ///     Attempts to draw a IDynamicCache Folder or FolderGroup by its IDynamicCollection counterpart.
    /// </summary>
    /// <param name="folder"> The Folder we want to draw the cached version of </param>
    /// <param name="width"> The width of the draw area. </param>
    /// <param name="groupIndent"> The indent to use for folder groups. </param>
    /// <param name="indent"> The indent to use for folders. </param>
    /// <param name="flags"> The flags to use for drawing. </param>
    public void DrawFolder(IDynamicCollection<T> folder, float width, float groupIndent, float indent, DynamicFlags flags = DynamicFlags.None)
    {
        // Ensure the child is at least draw to satisfy the expected drawn content region.
        using var _ = ImRaii.Child(Label, new Vector2(width, -1), false, WFlags.NoScrollbar);
        if (!_) return;

        // Handle any main context interactions such as right-click menus and the like.
        HandleMainContextActions();
        // Update the cache to its latest state.
        FilterCache.UpdateCache();

        if (!FilterCache.CacheMap.TryGetValue(folder, out var cachedNode))
            return;

        // Set the style for the draw logic.
        ImGui.SetScrollX(0);
        using var style = ImRaii.PushStyle(ImGuiStyleVar.WindowPadding, Vector2.One)
            .Push(ImGuiStyleVar.IndentSpacing, 14f * ImGuiHelpers.GlobalScale);
        // Draw out the node, for folders only.
        DrawClippedCacheNode(cachedNode, groupIndent, indent, flags);
        PostDraw();
    }

    /// <summary>
    ///     Add post-draw logic that is executed after drawing the full DynamicDrawSelector UI. <para />
    ///     Reserved for operations that would modify the cache state, or set the filter to dirty. <para />
    ///     If you override this make sure that the base is called, or else post-draw actions will not be processed.
    /// </summary>
    protected void PostDraw()
    {
        UpdateHoverNode();
//#if DEBUG
//        ImGui.Text($"Selected: {Selector.Collections.Count} Collections");
//        ImGui.Text($"Selected: {Selector.Leaves.Count} Leaves");
//        ImGui.Text($"Selected: {Selector.Selected.Count} Nodes");
//        ImGui.Text($"CacheMapSize: {FilterCache.CacheMap.Count}");
//        ImGui.Text($"FlatCacheSize: {FilterCache.FlatList.Count}");
//        ImGui.Text($"Total Cache Children: {FilterCache.RootCache.GetAllDescendants().Count()}");
//        ImGui.Text($"Total DragDrop Nodes: {DragDrop.Total}");
//        ImGui.Text($"DragDrop Names: {string.Join(',', DragDrop.Nodes.Select(n => n.Name))}");
//#endif
        // Process post-draw actions.
        while (_postDrawActions.TryDequeue(out Action? action))
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                Log.Warning($"HandleAction Error: {e}");
            }
        }
    }

    protected virtual void UpdateHoverNode()
    {
        _hoveredNode    = _newHoveredNode;
        _newHoveredNode = null;
    }

    // Helper for parent classes.
    protected void AddPostDrawLogic(Action act)
        => _postDrawActions.Enqueue(act);
}
