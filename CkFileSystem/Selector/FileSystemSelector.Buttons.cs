using CkCommons.Gui;
using Dalamud.Bindings.ImGui;
using OtterGui.Classes;

namespace CkCommons.FileSystem.Selector;

public partial class CkFileSystemSelector<T, TStateStorage>
{
    /// <summary> Draw necessary popups from buttons outside of pushed styles. </summary>
    public virtual void DrawPopups() { }

    /// <summary> Protected so it can be removed. </summary>
    protected void FolderAddButton()
    {
        const string newFolderName = "folderName";

        if (CkGui.IconButton(FAI.FolderPlus))
            ImGui.OpenPopup(newFolderName);
        CkGui.AttachToolTip("Create a new, empty folder. Can contain '/' to create a directory structure.");

        // Does not need to be delayed since it is not in the iteration itself.
        CkFileSystem<T>.Folder? folder = null;
        if (OpenRenamePopup(newFolderName, ref _newName) && _newName.Length > 0)
            try
            {
                folder = CkFileSystem.FindOrCreateAllFolders(_newName);
                _newName = string.Empty;
            }
            catch { /* Consume */ }

        if (folder != null)
            _filterDirty |= ExpandAncestors(folder);
    }

    // remove later maybe? Might be useful for multi- delete later idk.
    protected void DeleteSelectionButton(Vector2 size, DoubleModifier modifier, string singular, string plural, Action<T> delete)
    {
        bool keys        = modifier.IsActive();
        bool anySelected = _selectedPaths.Count > 1 || SelectedLeaf != null;
        string name        = _selectedPaths.Count > 1 ? plural : singular;
        string tt = !anySelected
            ? $"No {plural} selected."
            : $"Delete the currently selected {name} entirely from your drive.\n"
          + "This can not be undone.";
        if (!keys)
            tt += $"\nHold {modifier} while clicking to delete the {name}.";

        if (CkGui.IconButton(FAI.Trash, size.Y, disabled: !anySelected || !keys))
        {
            if (Selected != null)
                delete(Selected);
            else
                foreach (CkFileSystem<T>.Leaf leaf in _selectedPaths.OfType<CkFileSystem<T>.Leaf>())
                    delete(leaf.Value);
        }
        CkGui.AttachToolTip(tt);
    }
}
