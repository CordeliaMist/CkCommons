using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;

namespace CkCommons.DrawSystem.Selector;

public partial class DynamicDrawer<T>
{
    public static bool OpenRenamePopup(string popupName, ref string newName)
    {
        using ImRaii.IEndObject popup = ImRaii.Popup(popupName);
        if (!popup)
            return false;

        if (ImGui.IsKeyPressed(ImGuiKey.Escape))
            ImGui.CloseCurrentPopup();

        ImGui.SetNextItemWidth(300 * ImGuiHelpers.GlobalScale);
        if (ImGui.IsWindowAppearing())
            ImGui.SetKeyboardFocusHere();
        bool enterPressed = ImGui.InputTextWithHint("##newName", "Enter New Name...", ref newName, 512, ImGuiInputTextFlags.EnterReturnsTrue);

        if (!enterPressed)
            return false;

        ImGui.CloseCurrentPopup();
        return true;
    }

    /// <summary> Used for buttons and context menu entries. </summary>
    private static void RemovePrioritizedDelegate<TDelegate>(List<(TDelegate, int)> list, TDelegate action) where TDelegate : Delegate
    {
        int idxAction = list.FindIndex(p => p.Item1 == action);
        if (idxAction >= 0)
            list.RemoveAt(idxAction);
    }

    /// <summary> Used for buttons and context menu entries. </summary>
    private static void AddPrioritizedDelegate<TDelegate>(List<(TDelegate, int)> list, TDelegate action, int priority)
        where TDelegate : Delegate
    {
        int idxAction = list.FindIndex(p => p.Item1 == action);
        if (idxAction >= 0)
        {
            if (list[idxAction].Item2 == priority)
                return;

            list.RemoveAt(idxAction);
        }

        int idx = list.FindIndex(p => p.Item2 > priority);
        if (idx < 0)
            list.Add((action, priority));
        else
            list.Insert(idx, (action, priority));
    }

    /// <summary> Expand all ancestors of a given path, used for when new objects are created. </summary>
    /// <param name="path"> The Path to expand all its ancestors from. </param>
    /// <returns> If any state was changed. </returns>
    /// <remarks> Can only be executed from the main selector window due to ID computation. Handles only ImGui-state. </remarks>
    private bool ExpandAncestors(IDynamicNode<T> entity)
    {
        var parentFolders = entity.GetAncestors();
        foreach (var folder in parentFolders)
            DrawSystem.SetOpenState(folder, true);

        return true;
    }
}
