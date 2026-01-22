using CkCommons.Gui;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;
using OtterGui;
using OtterGui.Text;
using OtterGui.Text.EndObjects;

namespace CkCommons.DrawSystem.Selector;

// Drag-Drop Functionality & Helpers.
public partial class DynamicDrawer<T>
{
    protected bool IsDragging => DragDrop.IsActive;

    /// <summary>
    ///     Attaches a Drag-Drop source to the previously drawn item, with the given label. <para />
    ///     Labels are to be defined by the draw function call method.
    /// </summary>
    /// <param name="entity"> The Entity being handled as a drag-drop source. </param>
    /// <returns> If the payload source was marked. If it was, we should not return </returns>
    protected void AsDragDropSource(IDynamicNode<T> entity)
    {
        using var source = ImUtf8.DragDropSource();
        if (!source)
            return;

        // If we fail to set the payload, it implies we have started to move it,
        // so ensure item is selected for cases where we dragged without clicking.
        if (!DragDropSource.SetPayload(DragDrop.Label))
        {
            // Update selection if not updated.
            if (!entity.Equals(Selector.LastSelected))
                Selector.SelectSingle(entity, Selector.Selected.Contains(entity));
        }
        
        // Hover text for the drag drop can be shown here.
        // Customize display text later, maybe allow a custom virtual func for display text or something.
        CkGui.InlineSpacing();
        ImGui.Text(DragDrop.MoveString);
        PostDragSourceText(entity);
    }


    protected virtual void PostDragSourceText(IDynamicNode<T> entity)
    { }

    protected void AsDragDropTarget(IDynamicNode<T> entity)
    {
        using var target = ImRaii.DragDropTarget();
        if (!target)
            return;
        // If we are not dropping the opLabel, or the cache is not active, ignore this.
        if (!ImGuiUtil.IsDropping(DragDrop.Label) || !DragDrop.IsActive)
            return;

        Log.Information($"DragDrop is over: {entity.FullPath}.");
        // Enqueue after this draw-frame the full transfer of all paths.
        _postDrawActions.Enqueue(() =>
        {
            ProcessTransfer(entity);
            DragDrop.RefreshNodes();
        });
    }

    // Should maybe allow this to be overridden or something, but not sure.
    private void ProcessTransfer(IDynamicNode<T> target)
    {
        // If the transfer is not a valid transfer, ignore.
        if (DragDrop.Total is 0 || !DragDrop.IsValidTransfer(target))
        {
            Log.Verbose("DragDrop transfer returned early as it was not a valid drop.");
            return;
        }

        Log.Debug($"Processing drag-drop transfer of {DragDrop.Total} entities to target {target.FullPath}.");
        Log.Debug($"Transferring nodes [{string.Join(',', DragDrop.Nodes.Select(e => e.Name))}] to [{target.Name}]");
        PerformDrop(target);
    }

    /// <summary>
    ///     The main logic that is performed after a drag-drop is validated. <para />
    ///     Can be overridden to provide custom behavior on drop, but know what you're doing!
    /// </summary>
    protected virtual void PerformDrop(IDynamicNode<T> target)
    { }
}
