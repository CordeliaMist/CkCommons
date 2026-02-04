using CkCommons.Gui;
using CkCommons.Raii;
using CkCommons.Widgets;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility.Raii;
using OtterGui.Text;
using OtterGui.Text.EndObjects;

namespace CkCommons.DrawSystem.Selector;

// Handles the current draw state, and inner functions for drawing entities.
// This is also where a majority of customization points are exposed.
public partial class DynamicDrawer<T>
{
    // The below functions will be reworked later.
    // Draws out the entire filter row.
    public bool DrawFilterRow(float width, int length)
    {
        using (ImRaii.Group())
             DrawSearchBar(width, length);
        PostSearchBar();
        return FilterCache.IsFilterDirty;
    }

    /// <summary>
    ///     Overridable DynamicDrawer 'Header' Element. (Filter Search) <para />
    ///     By default, no additional options are shown outside of the search filter.
    /// </summary>
    protected virtual void DrawSearchBar(float width, int length)
    {
        var tmp = FilterCache.Filter;
        if (FancySearchBar.Draw("Filter", width, ref tmp, string.Empty, length))
            FilterCache.Filter = tmp;
    }

    protected virtual void PostSearchBar()
    { }

    #region Top Level CacheNodes Drawers
    /// <summary>
    ///     Highest level call to draw a cached <see cref="IDynamicCollection{T}"/> node from the draw system. <br/>
    ///     This node is expected to be filtered by the drawer / cache. <para />
    /// </summary>
    /// <param name="cachedNode"> The cached node to draw. </param>
    /// <param name="groupIndent"> The indent spacing given to DynamicGroupFolders. (0 for ignored) </param>
    /// <param name="indent"> The indent given to DynamicFolders. (0 for ignored) </param>
    /// <param name="flags"> The dynamic draw flags. </param>
    protected void DrawClippedCacheNode(IDynamicCache<T> cachedNode, float groupIndent, float indent, DynamicFlags flags)
    {
        if (cachedNode is DynamicFolderGroupCache<T> cfg)
            DrawClippedCacheNode(cfg, groupIndent, indent, flags);
        else if (cachedNode is DynamicFolderCache<T> cf)
            DrawClippedCacheNode(cf, indent, flags);
    }

    /// <summary>
    ///     Draws a cached <see cref="IDynamicCollection{T}"/> node from the draw system. (FolderGroup or Folder by default) <br/>
    ///     This node is expected to be filtered by the drawer / cache. <para />
    ///     Any folder not matching <typeparamref name="TFolder"/> is skipped.
    /// </summary>
    /// <param name="cachedNode"> The cached node to draw. </param>
    /// <param name="groupIndent"> The indent spacing given to DynamicGroupFolders. (0 for ignored) </param>
    /// <param name="indent"> The indent given to DynamicFolders. (0 for ignored) </param>
    /// <param name="flags"> The dynamic draw flags. </param>
    protected void DrawClippedCacheNode<TFolder>(IDynamicCache<T> cachedNode, float groupIndent, float indent, DynamicFlags flags)
        where TFolder : DynamicFolder<T>
    {
        if (cachedNode is DynamicFolderGroupCache<T> cfg)
            DrawClippedCacheNode(cfg, groupIndent, indent, flags);
        else if (cachedNode is DynamicFolderCache<T> cf && cf.Folder is TFolder)
            DrawClippedCacheNode(cf, indent, flags);
    }

    /// <summary>
    ///     Draws a cached <see cref="DynamicFolderGroupCache{T}"/> node from the drawer.<br/>
    ///     This node is expected to be filtered by the drawer / cache. <para />
    /// </summary>
    /// <param name="cfg"> The CachedFolderGroup node to draw. </param>
    /// <param name="groupIndent"> The indent spacing given to DynamicGroupFolders. (0 for ignored) </param>
    /// <param name="indent"> The indent given to DynamicFolders. (0 for ignored) </param>
    /// <param name="flags"> The dynamic draw flags. </param>
    protected void DrawClippedCacheNode(DynamicFolderGroupCache<T> cfg, float groupIndent, float indent, DynamicFlags flags)
    {
        using var id = ImRaii.PushId(Label + cfg.Folder.ID);
        DrawFolderGroupBanner(cfg.Folder, flags, _hoveredNode == cfg.Folder || Selector.Selected.Contains(cfg.Folder));
        if (flags.HasAny(DynamicFlags.DragDropFolders))
            AsDragDropTarget(cfg.Folder);

        using var tab = ImRaii.PushIndent(groupIndent, groupIndent != 0);
        DrawFolderGroupChildren(cfg, groupIndent, indent, flags);
    }

    /// <summary>
    ///     Draws a cached <see cref="DynamicFolderGroupCache{T}"/> node from the draw system. (FolderGroup or Folder by default) <br/>
    ///     This node is expected to be filtered by the drawer / cache. <para />
    ///     Any folder not matching <typeparamref name="TFolder"/> is skipped.
    /// </summary>
    /// <param name="cfg"> The CachedFolderGroup node to draw. </param>
    /// <param name="groupIndent"> The indent spacing given to DynamicGroupFolders. (0 for ignored) </param>
    /// <param name="indent"> The indent given to DynamicFolders. (0 for ignored) </param>
    /// <param name="flags"> The dynamic draw flags. </param>
    protected void DrawClippedCacheNode<TFolder>(DynamicFolderGroupCache<T> cfg, float groupIndent, float indent, DynamicFlags flags)
        where TFolder : DynamicFolder<T>
    {
        using var id = ImRaii.PushId(Label + cfg.Folder.ID);
        DrawFolderGroupBanner(cfg.Folder, flags, _hoveredNode == cfg.Folder || Selector.Selected.Contains(cfg.Folder));
        if (flags.HasAny(DynamicFlags.DragDropFolders))
            AsDragDropTarget(cfg.Folder);
        // Draw the children objects.
        using var tab = ImRaii.PushIndent(groupIndent, false, groupIndent != 0);
        DrawFolderGroupChildren<TFolder>(cfg, groupIndent, indent, flags);
    }

    /// <summary>
    ///     The clipped draw method for folder groups. <para />
    ///     The parent's children are not drawn if the parent's children are not visible.
    /// </summary>
    /// <returns> True if the parent folder was visible, false otherwise. </returns>
    protected void DrawClippedCacheNode(DynamicFolderCache<T> cf, float indent, DynamicFlags flags)
    {
        using var id = ImRaii.PushId($"DDS_{Label}_{cf.Folder.ID}");
        DrawFolderBanner(cf.Folder, flags, _hoveredNode == cf.Folder || Selector.Selected.Contains(cf.Folder));
        if (flags.HasAny(DynamicFlags.DragDropFolders))
            AsDragDropTarget(cf.Folder);

        if (flags.HasAny(DynamicFlags.FoldersOnly))
            return;

        // Draw the children objects.
        using var _ = ImRaii.PushIndent(indent, false, indent != 0);
        DrawFolderLeaves(cf, flags);
    }

    #endregion Top Level CacheNodes Drawers

    #region DrawFolder Headers / Banners
    // Overridable draw method for the folder display.
    protected virtual void DrawFolderGroupBanner(IDynamicFolderGroup<T> fg, DynamicFlags flags, bool selected)
    {
        var width = CkGui.GetWindowContentRegionWidth() - ImGui.GetCursorPosX();
        var bgCol = selected ? ImGui.GetColorU32(ImGuiCol.FrameBgHovered) : fg.BgColor;
        // Display a framed child with stylizations based on the folders preferences.
        using (var _ = CkRaii.FramedChildPaddedW($"dfg_{Label}_{fg.ID}", width, ImUtf8.FrameHeight, bgCol, fg.BorderColor, 5f, 1f))
        {
            DrawFolderGroupBanner(fg, _.InnerRegion, flags);
            HandleContext(fg);
        }
    }

    /// <summary>
    ///     The visible folder draw region.
    /// </summary>
    /// <param name="fg"> The folder group to draw. </param>
    /// <param name="region"> The size of the folder draw region. </param>
    /// <param name="flags"> The dynamic draw flags. </param>
    protected virtual void DrawFolderGroupBanner(IDynamicFolderGroup<T> fg, Vector2 region, DynamicFlags flags)
    {
        var pos = ImGui.GetCursorPos();
        if (ImGui.InvisibleButton($"{Label}_node_{fg.ID}", region))
            HandleLeftClick(fg, flags);
        HandleDetections(fg, flags);

        // Back to the start of the line, then draw the folder display contents.
        ImGui.SameLine(pos.X);
        CkGui.FramedIconText(fg.IsOpen ? fg.IconOpen : fg.Icon);
        CkGui.ColorTextFrameAlignedInline(fg.Name, fg.NameColor);
    }

    /// <summary>
    ///     Draws the child nodes of <see cref="DynamicFolderGroup{T}"/>. Can be customized. <para />
    /// </summary>
    /// <param name="cfg"> The cached folder group to draw. </param>
    /// <param name="groupIndent"> The indent spacing given to DynamicGroupFolders. (0 for ignored) </param>
    /// <param name="indent"> The indent given to DynamicFolders. (0 for ignored) </param>
    /// <param name="flags"> The dynamic draw flags. </param>
    protected virtual void DrawFolderGroupChildren(DynamicFolderGroupCache<T> cfg, float groupIndent, float indent, DynamicFlags flags)
    {
        foreach (var child in cfg.Children)
            DrawClippedCacheNode(child, groupIndent, indent, flags);
    }

    // For Future Reference.
    //protected virtual void DrawFolderGroupChildren(DynamicFolderGroupCache<T> cfg, float groupIndent, float indent, DynamicFlags flags)
    //{
    //    if (cfg.Folder.IsRoot)
    //    {
    //        // Simple for-each loop, if things ever really become a problem we can ClipRect this, but not much of an issue for now.
    //        foreach (var child in cfg.Children)
    //            DrawClippedCacheNode(child, groupIndent, indent, flags);
    //    }
    //    else
    //    {
    //        // Draw the children objects.
    //        using var tab = ImRaii.PushIndent(groupIndent, groupIndent != 0);
    //        // Folder line stuff.
    //        var offsetX = -groupIndent + ImGui.GetTreeNodeToLabelSpacing() / 2;
    //        var lineStart = ImGui.GetCursorScreenPos();
    //        var drawList = ImGui.GetWindowDrawList();
    //        lineStart.X += offsetX;
    //        lineStart.Y -= 2 * ImGuiHelpers.GlobalScale;
    //        var lineEnd = lineStart;

    //        foreach (var child in cfg.Children)
    //        {
    //            var lineSize = Math.Max(0, groupIndent - 9 * ImGuiHelpers.GlobalScale);
    //            // Draw the child
    //            DrawClippedCacheNode(child, groupIndent, indent, flags);
    //            var minRect = ImGui.GetItemRectMin();
    //            var maxRect = ImGui.GetItemRectMax();
    //            if (minRect.X == 0)
    //                continue;

    //            lineEnd.Y = maxRect.Y;
    //        }
    //        // Finally, draw the folder line.
    //        drawList.AddLine(lineStart, lineEnd, cfg.Folder.BorderColor, ImGuiHelpers.GlobalScale);
    //    }
    //}


    /// <summary>
    ///     Draws the child nodes of <see cref="DynamicFolderGroup{T}"/>. Can be customized. <para />
    ///     You can use a <typeparamref name="TFolder"/> arguement make the drawer only display folders of that type.
    /// </summary>
    /// <param name="cfg"> The cached folder group to draw. </param>
    /// <param name="groupIndent"> The indent spacing given to DynamicGroupFolders. (0 for ignored) </param>
    /// <param name="indent"> The indent given to DynamicFolders. (0 for ignored) </param>
    /// <param name="flags"> The dynamic draw flags. </param>
    protected virtual void DrawFolderGroupChildren<TFolder>(DynamicFolderGroupCache<T> cfg, float groupIndent, float indent, DynamicFlags flags) 
        where TFolder : DynamicFolder<T>
    {
        // Simple for-each loop, if things ever really become a problem we can ClipRect this, but not much of an issue for now.
        foreach (var child in cfg.Children)
            DrawClippedCacheNode<TFolder>(child, groupIndent, indent, flags);
    }

    protected virtual void DrawFolderBanner(IDynamicFolder<T> f, DynamicFlags flags, bool selected)
    {
        // We could likely reduce this by a lot if we had a override for this clipped draw within the dynamic draw system.
        var width = CkGui.GetWindowContentRegionWidth() - ImGui.GetCursorPosX();
        var bgCol = selected ? ImGui.GetColorU32(ImGuiCol.FrameBgHovered) : f.BgColor;
        // Display a framed child with stylizations based on the folders preferences.
        using (var _ = CkRaii.FramedChildPaddedW($"df_{Label}_{f.ID}", width, ImUtf8.FrameHeight, bgCol, f.BorderColor, 5f, 1f))
        {
            DrawFolderBannerInner(f, _.InnerRegion, flags);
            HandleContext(f);
        }
    }

    protected virtual void DrawFolderBannerInner(IDynamicFolder<T> f, Vector2 region, DynamicFlags flags)
    {
        var pos = ImGui.GetCursorPos();
        if (ImGui.InvisibleButton($"{Label}_node_{f.ID}", region))
            HandleLeftClick(f, flags);
        HandleDetections(f, flags);

        // Back to the start of the line, then draw the folder display contents.
        ImGui.SameLine(pos.X);
        CkGui.FramedIconText(f.IsOpen ? FAI.CaretDown : FAI.CaretRight);
        ImGui.SameLine();
        CkGui.IconTextAligned(f.Icon, f.IconColor);
        CkGui.ColorTextFrameAlignedInline(f.Name, f.NameColor);
    }
    #endregion DrawFolder Headers / Banners


    // Outer, customization point for styling.
    protected virtual void DrawFolderLeaves(DynamicFolderCache<T> cf, DynamicFlags flags)
    {
        var folderMin = ImGui.GetItemRectMin();
        var folderMax = ImGui.GetItemRectMax();
        var wdl = ImGui.GetWindowDrawList();
        wdl.ChannelsSplit(2);
        wdl.ChannelsSetCurrent(1);

        // Should make this have variable heights later.
        ClippedDraw(cf.Children, DrawLeafClipped, ImUtf8.FrameHeightSpacing, flags);

        wdl.ChannelsSetCurrent(0); // Background.
        var gradientTL = new Vector2(folderMin.X, folderMax.Y);
        var gradientTR = new Vector2(folderMax.X, ImGui.GetItemRectMax().Y);
        wdl.AddRectFilledMultiColor(gradientTL, gradientTR, ColorHelpers.Fade(cf.Folder.GradientColor, .9f), ColorHelpers.Fade(cf.Folder.GradientColor, .9f), 0, 0);
        wdl.ChannelsMerge();
    }

    // Adapter used by the clipper so we don't allocate a lambda capturing locals each frame.
    protected void DrawLeafClipped(IDynamicLeaf<T> leaf, DynamicFlags flags)
        => DrawLeaf(leaf, flags, leaf.Equals(_hoveredNode) || Selector.Selected.Contains(leaf));

    protected virtual void DrawLeaf(IDynamicLeaf<T> leaf, DynamicFlags flags, bool selected)
    {
        var size = new Vector2(CkGui.GetWindowContentRegionWidth() - ImGui.GetCursorPosX(), ImUtf8.FrameHeight);
        var bgCol = selected ? ImGui.GetColorU32(ImGuiCol.FrameBgHovered) : 0;

        // The DrawContext MUST be drawn within this child to remain in scope.
        using (var _ = CkRaii.Child(Label + leaf.Name, size, bgCol, 5f))
        {
            DrawLeafInner(leaf, _.InnerRegion, flags);
            HandleContext(leaf);
        }
    }

    protected virtual void DrawLeafInner(IDynamicLeaf<T> leaf, Vector2 region, DynamicFlags flags)
    {
        var pos = ImGui.GetCursorPos();
        if (ImGui.InvisibleButton($"{Label}_node_{leaf.Name}", region))
            HandleLeftClick(leaf, flags);
        HandleDetections(leaf, flags);

        ImGui.SameLine(pos.X);
        CkGui.TextFrameAligned(leaf.Name);
    }

    /// <summary>
    ///     Context Menu displayed when the selector background is right-clicked. <br/>
    /// </summary>
    protected void HandleMainContext()
    {
        if (!ImGui.IsAnyItemHovered() && ImGui.IsMouseClicked(ImGuiMouseButton.Right) && ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
        {
            if (!ImGui.IsWindowFocused(ImGuiFocusedFlags.RootAndChildWindows))
                ImGui.SetWindowFocus(Label);
            ImGui.OpenPopup("MainContext");
        }

        using var _ = ImRaii.Popup("MainContext");
        if (!_) return; 
        DrawContextMenu();
    }

    protected void HandleContext(IDynamicFolderGroup<T> node)
    {
        using var popup = ImRaii.Popup(node.FullPath);
        if (popup)
        {
            _popupNodes.Add(node);
            DrawContextMenu(node);
        }
        else
        {
            _popupNodes.Remove(node);
        }
    }

    protected void HandleContext(IDynamicFolder<T> node)
    {
        using var popup = ImRaii.Popup(node.FullPath);
        if (popup)
        {
            _popupNodes.Add(node);
            DrawContextMenu(node);
        }
        else
        {
            _popupNodes.Remove(node);
        }
    }

    protected void HandleContext(IDynamicLeaf<T> node)
    {
        using var popup = ImRaii.Popup(node.FullPath);
        if (popup)
        {
            _popupNodes.Add(node);
            DrawContextMenu(node);
        }
        else
        {
            _popupNodes.Remove(node);
        }
    }

    /// <summary>
    ///     The Selector's Context Menu.
    /// </summary>
    protected virtual void DrawContextMenu()
    {
        if (ImGui.MenuItem("Expand All"))
            AddPostDrawLogic(() => ToggleDescendants(DrawSystem.Root, true));

        if (ImGui.MenuItem("Collapse All"))
            AddPostDrawLogic(() => ToggleDescendants(DrawSystem.Root, false));
    }

    /// <summary>
    ///     The FolderGroup Context Menu.
    /// </summary>
    protected virtual void DrawContextMenu(IDynamicFolderGroup<T> node)
    {
        if (ImGui.MenuItem("Expand Descendants"))
            AddPostDrawLogic(() => ToggleDescendants(node, true));
        CkGui.AttachToolTip("Expand all child collection nodes.");

        if (ImGui.MenuItem("Collapse Descendants"))
            AddPostDrawLogic(() => ToggleDescendants(node, false));
        CkGui.AttachToolTip("Collapse all child collection nodes.");

        if (ImGui.MenuItem("Dissolve", enabled: ImGui.GetIO().KeyShift))
            AddPostDrawLogic(() => DrawSystem.Delete(node));
        CkGui.AttachToolTip("Remove this FolderGroup, then move all children to the parent node." +
            "--SEP----COL--Hold SHIFT to delete.--COL--", ImGuiColors.DalamudYellow);
    }

    /// <summary>
    ///     The Folder Context Menu.
    /// </summary>
    protected virtual void DrawContextMenu(IDynamicFolder<T> node)
    {
        // Nothing by default, close immidiately.
        ImGui.CloseCurrentPopup(); // This causes an issue when closed in the same frame, find better alternative.
    }

    /// <summary>
    ///     The Leaf Context Menu.
    /// </summary>
    protected virtual void DrawContextMenu(IDynamicLeaf<T> node)
    {
        // Nothing by default, close immidiately.
        ImGui.CloseCurrentPopup(); // This causes an issue when closed in the same frame, find better alternative.
    }

    /// <summary>
    ///     Left-Click handling is processed seperately from detections.
    ///     This is because based on what's drawn by the DrawInner() func,
    ///     how a completed 'click' is determined can differ. <para />
    ///     Buttons return true after OnMousePressed() -> OnMouseRelease(), while IsItemClicked() 
    ///     is true on MouseDown() <para />
    ///     <b> Overriding these implies you know what you are doing. </b>
    /// </summary>
    protected virtual void HandleLeftClick(IDynamicCollection<T> node, DynamicFlags flags)
    {
        // Handle Folder Toggle.
        DrawSystem.SetOpenState(node, !node.IsOpen);

        // Handle Selection.
        if (flags.HasAny(DynamicFlags.SelectableFolders))
            Selector.SelectItem(node, flags.HasFlag(DynamicFlags.MultiSelect), flags.HasFlag(DynamicFlags.RangeSelect));
    }

    /// <inheritdoc cref="HandleLeftClick(IDynamicCollection{T},DynamicFlags)"/>
    protected virtual void HandleLeftClick(IDynamicLeaf<T> node, DynamicFlags flags)
    {
        // Handle Selection.
        if (flags.HasAny(DynamicFlags.SelectableLeaves))
            Selector.SelectItem(node, flags.HasFlag(DynamicFlags.MultiSelect), flags.HasFlag(DynamicFlags.RangeSelect));
    }

    /// <summary>
    ///     Defines hover, drag-drop, and other detection logic for a node. <para />
    /// </summary>
    protected virtual void HandleDetections(IDynamicCollection<T> node, DynamicFlags flags)
    {
        if (ImGui.IsItemHovered())
            _newHoveredNode = node;

        // Handle Drag and Drop.
        if (flags.HasAny(DynamicFlags.DragDropFolders))
        {
            AsDragDropSource(node);
            AsDragDropTarget(node);
        }

        // Handle Context Menus. (Maybe make a flag later. Would save on some drawtime.)
        if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
            ImGui.OpenPopup(node.FullPath);
    }

    /// <inheritdoc cref="HandleDetections(IDynamicCollection{T},DynamicFlags)"/>
    protected virtual void HandleDetections(IDynamicLeaf<T> node, DynamicFlags flags)
    {
        if (ImGui.IsItemHovered())
            _newHoveredNode = node;

        // Handle Drag and Drop.
        if (flags.HasAny(DynamicFlags.DragDropLeaves))
        {
            AsDragDropSource(node);
            AsDragDropTarget(node);
        }

        // Handle Context Menus. (Maybe make a flag later. Would save on some drawtime.)
        if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
            ImGui.OpenPopup(node.FullPath);
    }

    // Special clipped draw just for the DynamicDrawer.
    protected void ClippedDraw<I>(IReadOnlyList<I> data, Action<I, DynamicFlags> draw, float lineHeight, DynamicFlags flags)
    {
        using var clipper = ImUtf8.ListClipper(data.Count, lineHeight);
        while (clipper.Step())
        {
            for (var actualRow = clipper.DisplayStart; actualRow < clipper.DisplayEnd; actualRow++)
            {
                if (actualRow >= data.Count)
                    return;

                if (actualRow < 0)
                    continue;

                draw(data[actualRow], flags);
            }
        }
    }

    // For drawing recursive FolderGroups
    protected void DynamicClippedDraw<I>(IReadOnlyList<I> data, Action<I, DynamicFlags> draw, DynamicFlags flags)
    {
        using IEnumerator<I> enumerator = data.GetEnumerator();

        int index = 0;
        int firstVisible = -1;
        int lastVisible = -1;

        while (enumerator.MoveNext())
        {
            // draw to check for visibility.
            draw(enumerator.Current, flags);
            // If the item is not visible, we can skip it.
            if (ImGui.IsItemVisible())
            {
                if (firstVisible == -1)
                    firstVisible = index;

                lastVisible = index;
            }
            else if (firstVisible != -1)
            {
                // went beyond the total visible range, so break out.
                break;
            }
            index++;
        }
    }
}
