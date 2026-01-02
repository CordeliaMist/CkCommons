using Dalamud.Bindings.ImGui;
using Dalamud.Interface;

namespace CkCommons.DrawSystem;

/// <summary>
///     Publicly accessible Folder for a DynamicDrawSystem node. <para />
///     All functions and setters are only accessible internally to ensure integrity.
/// </summary>
public class DynamicFolderGroup<T> : IDynamicFolderGroup<T> where T : class
{
    public string StringSplitter => "//";
    public DynamicFolderGroup<T> Parent { get; internal set; }
    public int          Priority => 0;
    public uint         ID       { get; }
    public string       Name     { get; internal set; }
    public string       FullPath { get; internal set; } = string.Empty;
    public FolderFlags  Flags    { get; internal set; } = FolderFlags.None;

    // Stylizations.
    public uint NameColor     { get; internal set; } = uint.MaxValue;
    public FAI  Icon          { get; internal set; } // Cant see any reason why this would be anything besides Folder
    public FAI  IconOpen      { get; internal set; } // Cant see any reason why this would be anything besides FolderOpen
    public uint IconColor     { get; internal set; } = uint.MaxValue;
    public uint BgColor       { get; internal set; } = uint.MinValue;
    public uint BorderColor   { get; internal set; } = uint.MaxValue;
    public uint GradientColor { get; internal set; } = ColorHelpers.Fade(ImGui.GetColorU32(ImGuiCol.TextDisabled), .9f);

    internal DynamicSorter<IDynamicCollection<T>> Sorter;
    internal List<IDynamicCollection<T>> Children = [];

    internal DynamicFolderGroup(DynamicFolderGroup<T> parent, uint id, string name, 
        FAI icon = FAI.Folder, FAI iconOpen = FAI.FolderOpen, 
        DynamicSorter<IDynamicCollection<T>>? sorter = null, FolderFlags flags = FolderFlags.None)
    {
        Parent = parent;
        Icon = icon;
        IconOpen = iconOpen;
        Name = name.FixName();
        ID = id;
        Flags = flags;
        Sorter = sorter ?? new();
        UpdateFullPath();

        //Svc.Logger.Information($"Created FolderGroup:\n" +
        //    $" Parent: {(Parent is null ? "null" : Parent.Name)}\n" +
        //    $" Name: {Name}\n" +
        //    $" ID: {ID}\n" +
        //    $" FullPath: {FullPath}\n" +
        //    $" Flags: {Flags}");
    }

    IReadOnlyDynamicSorter<IDynamicCollection<T>> IDynamicFolderGroup<T>.Sorter => Sorter;
    IReadOnlyList<IDynamicCollection<T>> IDynamicFolderGroup<T>.Children => Children;

    public int TotalChildren => Children.Count;
    public bool IsRoot => string.Equals(Name, DDSHelpers.RootName, StringComparison.Ordinal);
    public bool IsOpen => Flags.HasAny(FolderFlags.Expanded);
    public bool ShowIfEmpty => Flags.HasAny(FolderFlags.ShowIfEmpty);

    public IReadOnlyList<IDynamicCollection<T>> GetChildren()
        => Children;

    public IEnumerable<DynamicFolderGroup<T>> GetSubFolderGroups()
        => Children.OfType<DynamicFolderGroup<T>>();

    public IEnumerable<DynamicFolder<T>> GetSubFolders()
        => Children.OfType<DynamicFolder<T>>();

    public IEnumerable<IDynamicCollection<T>> GetAllFolderDescendants()
    {
        return Children.SelectMany(p =>
        {
            if (p is DynamicFolderGroup<T> fc)
                return fc.GetAllFolderDescendants().Prepend(fc);
            else if (p is DynamicFolder<T> f)
                return [ f ];
            else
                return Array.Empty<IDynamicCollection<T>>();
        });
    }

    // Iterate through all Descendants in sort order, not including the folder itself.
    public IEnumerable<IDynamicNode<T>> GetAllDescendants()
    {
        return Children.SelectMany(p =>
        {
            if (p is DynamicFolderGroup<T> fc)
                return fc.GetAllDescendants().Prepend(fc);
            else if (p is DynamicFolder<T> f)
                return f.Children.Cast<IDynamicNode<T>>().Prepend(f);
            else
                return Array.Empty<IDynamicNode<T>>().Append(p);
        });
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

    public override string ToString()
        => Name;

    /// <summary>
    ///     Adds a child to the FolderGroup. (Validation not checked) <para />
    ///     The added child decouples itself from it's previous parent. <para />
    /// </summary>
    internal void AddChild(IDynamicCollection<T> child)
    {
        child.Parent.Children.Remove(child);
        Children.Add(child);
        if (child is DynamicFolderGroup<T> fc)
        {
            fc.Parent = this;
            fc.UpdateFullPath();
        }
        else if (child is DynamicFolder<T> f)
        {
            f.Parent = this;
            f.UpdateFullPath();
        }
        SortChildren();
    }

    /// <summary>
    ///     Adds all children to the FolderGroup, does not sort after and must be called.
    /// </summary>
    internal void AddChildren(IEnumerable<IDynamicCollection<T>> children)
    {
        Children.AddRange(children);
        foreach (var child in children)
        {
            if (child is DynamicFolderGroup<T> fc)
            {
                fc.Parent.Children.Remove(fc);
                fc.Parent = this;
                fc.UpdateFullPath();
            }
            else if (child is DynamicFolder<T> f)
            {
                f.Parent.Children.Remove(f);
                f.Parent = this;
                f.UpdateFullPath();
            }
        }
    }

    /// <summary>
    ///     Inserts all children at a selected IDX then sorts by node type.
    /// </summary>
    internal void InsertChildren(IEnumerable<IDynamicCollection<T>> children, int idx)
    {
        // Add them and update their parent / full path.
        Children.InsertRange(idx, children);
        foreach (var child in children)
        {
            if (child is DynamicFolderGroup<T> fc)
            {
                fc.Parent.Children.Remove(fc);
                fc.Parent = this;
                fc.UpdateFullPath();
            }
            else if (child is DynamicFolder<T> f)
            {
                f.Parent.Children.Remove(f);
                f.Parent = this;
                f.UpdateFullPath();
            }
        }
    }

    internal void SetName(string name, bool fix, bool forceSort = false)
    {
        Name = fix ? name.FixName() : name;
        UpdateFullPath();
    }

    /// <summary>
    ///     Sort by priority, FolderGroups, Folders, then Leaves, then whatever else.
    /// </summary>
    internal void SortChildren()
        => Children.Sort((a, b) => a.Priority.CompareTo(b.Priority));

    internal void SetIsOpen(bool value)
        => Flags = value ? Flags | FolderFlags.Expanded : Flags & ~FolderFlags.Expanded;

    internal void SetShowEmpty(bool value)
        => Flags = value ? Flags | FolderFlags.ShowIfEmpty : Flags & ~FolderFlags.ShowIfEmpty;

    internal void UpdateFullPath()
    {
        if (IsRoot) return;
        // construct the string builder and begin concatenation.
        var sb = new StringBuilder();
        // call recursive concatenation across ancestors.
        IDynamicCollection<T>.Concat(this, sb);
        // build the string and update it.
        FullPath = sb.ToString();

        // Update all children's full paths (maybe revise this later or something)
        foreach (var child in Children)
        {
            if (child is DynamicFolderGroup<T> fc)
                fc.UpdateFullPath();
            else if (child is DynamicFolder<T> f)
                f.UpdateFullPath();
        }
    }

    // Creates the root folder collection of the dynamic folder system.
    public static DynamicFolderGroup<T> CreateRoot(IEnumerable<ISortMethod<IDynamicCollection<T>>>? sorter = null)
        => new(null!, 0, DDSHelpers.RootName, FAI.Folder, sorter: new(sorter ?? []), flags: FolderFlags.All);
}