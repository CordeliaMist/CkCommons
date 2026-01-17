namespace CkCommons.DrawSystem;

/// <summary>
///     Flags that determine a folder's display behavior.
/// </summary>
[Flags]
public enum FolderFlags : byte
{
    None        = 0 << 0,
    Expanded    = 1 << 0, // If the folder is expanded.
    ShowIfEmpty = 1 << 1, // If the folder should display even with 0 children.

    All = Expanded | ShowIfEmpty, // Default flags for the root folder.
}

[Flags]
public enum DynamicFlags : uint
{
    None                = 0 << 0, // No behaviors are set for this draw.
    SelectableFolders   = 1 << 1, // Folder Single-Select is allowed.
    SelectableLeaves    = 1 << 2, // Leaf Single-Select is allowed.
    MultiSelect         = 1 << 3, // Multi-Selection (anchoring / CTRL) is allowed.
    RangeSelect         = 1 << 4, // Range Selection (SHIFT) is allowed.
    DragDropFolders     = 1 << 5, // Folder Drag-Drop is allowed.
    DragDropLeaves      = 1 << 6, // Leaf Drag-Drop is allowed.
    CopyDrag            = 1 << 7, // Drag-Drop performs copy on the dragged items over a move.
    TrashDrop           = 1 << 8, // Drag-Drop removes the source items on drop into another target, instead of moving.
    FoldersOnly         = 1 << 9, // Only folders are displayed.
    // Could add ContextFolderGroups, ContextFolders, ContextLeaves here, but wait until later.

    // Masks
    Selectable = SelectableLeaves | MultiSelect | RangeSelect,
    SelectableDragDrop = SelectableFolders | MultiSelect | RangeSelect | DragDropFolders | FoldersOnly,
    GroupEditor = SelectableLeaves | MultiSelect | RangeSelect | DragDropLeaves,
    AllEditor = SelectableLeaves | MultiSelect | RangeSelect | DragDropLeaves | CopyDrag | TrashDrop,
    DragDrop = DragDropFolders | DragDropLeaves,
}
