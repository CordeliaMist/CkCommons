using Dalamud.Bindings.ImGui;

namespace CkCommons.DrawSystem;

/// <summary>
///     A folder serves as a reference for all generated leaves within it. Refreshed with a refresh call. <para />
///     Serves as a base class for others to be built upon.
/// </summary>
public abstract class DynamicFolder<T> : IDynamicFolder<T> where T : class
{
    private Dictionary<T, DynamicLeaf<T>> _map = new();

    public string StringSplitter => "/";
    public DynamicFolderGroup<T> Parent { get; internal set; }
    public int         Priority => 1;
    public uint        ID       { get; internal set; }
    public string      Name     { get; internal set; }
    public string      FullPath { get; internal set; } = string.Empty;
    public FolderFlags Flags    { get; private set; } = FolderFlags.None;

    // Stylizations.
    // (Can be protected as the cached folder uses the base ref)
    public uint NameColor     { get; protected set; } = uint.MaxValue;
    public FAI  Icon          { get; protected set; } = FAI.Folder;
    public uint IconColor     { get; protected set; } = uint.MaxValue;
    public uint BgColor       { get; protected set; } = uint.MinValue;
    public uint BorderColor   { get; protected set; } = ImGui.GetColorU32(ImGuiCol.TextDisabled);
    public uint GradientColor { get; protected set; } = ImGui.GetColorU32(ImGuiCol.TextDisabled);

    protected internal DynamicSorter<DynamicLeaf<T>> Sorter;
    protected internal List<DynamicLeaf<T>> Children = [];

    public DynamicFolder(DynamicFolderGroup<T> parent, FAI icon, string name, uint id,
        DynamicSorter<DynamicLeaf<T>>? sorter = null, FolderFlags flags = FolderFlags.None)
    {
        ID = id;
        Parent = parent;
        Icon = icon;
        Name = name.FixName();
        Flags = flags;
        Sorter = sorter ?? new();
        UpdateFullPath();
        //Svc.Logger.Information($"Created Folder:\n" +
        //    $" Parent: {(Parent.Name)}\n" +
        //    $" Name: {Name}\n" +
        //    $" ID: {ID}\n" +
        //    $" FullPath: {FullPath}\n" +
        //    $" Flags: {Flags}");
    }

    IReadOnlyDynamicSorter<DynamicLeaf<T>> IDynamicFolder<T>.Sorter => Sorter;
    IReadOnlyList<DynamicLeaf<T>> IDynamicFolder<T>.Children => Children;

    public int TotalChildren
        => Children.Count;

    public bool IsRoot
        => false;

    public bool IsOpen
        => Flags.HasAny(FolderFlags.Expanded);

    public bool ShowIfEmpty
        => Flags.HasAny(FolderFlags.ShowIfEmpty);

    public IReadOnlyList<DynamicLeaf<T>> GetChildren()
        => Children;

    public IReadOnlyList<IDynamicCollection<T>> GetAncestors()
    {
        if (Parent is null || Parent.IsRoot)
            return Array.Empty<IDynamicCollection<T>>();

        var ancestors = new List<IDynamicCollection<T>>();
        for (IDynamicCollection<T> cur = Parent; !cur.IsRoot; cur = cur.Parent)
            ancestors.Add(cur);

        return ancestors;
    }

    // Internal Helpers.
    internal bool Update(out List<DynamicLeaf<T>> removed)
    {
        removed = [];
        var latest = GetAllItems();

        // Remove items no longer present.
        foreach (var key in _map.Keys.ToList())
        {
            if (!latest.Contains(key))
            {
                removed.Add(_map[key]);
                _map.Remove(key);
            }
        }

        // Add the new items that were not present before.
        foreach (var item in latest)
        {
            if (!_map.ContainsKey(item))
                _map[item] = ToLeaf(item);
        }

        // Update the items.
        Children = _map.Values.OrderBy(x => x.Name).ToList();
        return removed.Any();
    }

    internal void SetName(string name, bool fix, bool forceSort = false)
    {
        Name = fix ? name.FixName() : name;
        UpdateFullPath();
    }

    internal void SetIsOpen(bool value)
        => Flags = value ? Flags | FolderFlags.Expanded : Flags & ~FolderFlags.Expanded;

    internal void SetShowEmpty(bool value)
        => Flags = value ? Flags | FolderFlags.ShowIfEmpty : Flags & ~FolderFlags.ShowIfEmpty;

    internal void UpdateFullPath()
    {
        // construct the string builder and begin concatenation.
        var sb = new StringBuilder();
        // call recursive concatenation across ancestors.
        IDynamicCollection<T>.Concat(this, sb);
        // build the string and update it.
        FullPath = sb.ToString();

        // Update all leaves for their new full path.
        // (Extremely lightweight)
        foreach (var leaf in Children)
            leaf.UpdateFullPath();
    }

    /// <summary>
    ///     Abstract method to obtain all items for the folder.
    /// </summary>
    protected abstract IReadOnlyList<T> GetAllItems();

    /// <summary>
    ///     Abstract method to determine how leaves are generated for this folder. <para />
    ///     You can technically break the hierarchy if you set the parent to anything 
    ///     but this folder's parent, so dont do that.
    /// </summary>
    protected abstract DynamicLeaf<T> ToLeaf(T item);

    public override string ToString()
        => Name;
}