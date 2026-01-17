using CkCommons.Gui;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;

namespace CkCommons.DrawSystem.Selector;

public partial class DynamicDrawer<T>
{
    // There was a prioritized Delegate system used for context menus within OtterGui's FileSystem,
    // fallback to this if we find the DDS approach too constricting.

    // Additionally, if we need to, the below functions can become protected if nessisary.

    /// <summary>
    ///     Expand all ancestors of a given path, used for when new objects are created.
    /// </summary>
    /// <returns> If any state was changed. </returns>
    private bool ExpandAncestors(IDynamicNode<T> entity)
    {
        var parentFolders = entity.GetAncestors();
        foreach (var folder in parentFolders)
            DrawSystem.SetOpenState(folder, true);

        return true;
    }

    protected void ToggleDescendants(IDynamicCollection<T> collection, bool newState)
    {
        // Set the state of the collection itself.
        DrawSystem.SetOpenState(collection, newState);
        
        // Then operate on all it's children.
        if (collection is IDynamicFolder<T> folder)
        {
            DrawSystem.SetOpenState(folder, newState);
        }
        else if (collection is DynamicFolderGroup<T> folderGroup)
        {
            foreach (var child in folderGroup.GetAllFolderDescendants())
                DrawSystem.SetOpenState(child, newState);
        }
    }
}
