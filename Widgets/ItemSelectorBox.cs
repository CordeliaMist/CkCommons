using CkCommons.Gui;
using CkCommons.Helpers;
using CkCommons.Raii;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using ImGuiNET;
using OtterGui.Extensions;

namespace CkCommons.Widgets;

// This is all still very WIP and subject to optimization changes.
public class ItemSelectorBoxBuilder<T>
{
    private FAI _addIcon;
    private string _addIconText = string.Empty;
    private uint? _colSelected;
    private uint? _colHovered;
    private uint? _bgCol;
    private Action<T>? _onSelect;
    private Action<T>? _onRemove;
    private Action<T, string>? _onRename;
    private Action<string>? _onAdd;

    public ItemSelectorBoxBuilder<T> Create(FAI addIcon, string addIconText = "")
    {
        _addIcon = addIcon;
        _addIconText = addIconText;
        return this;
    }

    public ItemSelectorBoxBuilder<T> WithColors(uint selectedCol, uint hoveredCol, uint bgCol)
    {
        _colSelected = selectedCol;
        _colHovered = hoveredCol;
        _bgCol = bgCol;
        return this;
    }

    public ItemSelectorBoxBuilder<T> WithColSelected(uint selectedCol)
    {
        _colSelected = selectedCol;
        return this;
    }

    public ItemSelectorBoxBuilder<T> WithColHovered(uint hoveredCol)
    {
        _colHovered = hoveredCol;
        return this;
    }

    public ItemSelectorBoxBuilder<T> WithBgCol(uint bgCol)
    {
        _bgCol = bgCol;
        return this;
    }

    public ItemSelectorBoxBuilder<T> OnAdd(Action<string> onAdd)
    {
        _onAdd = onAdd;
        return this;
    }

    public ItemSelectorBoxBuilder<T> OnRename(Action<T, string> onRename)
    {
        _onRename = onRename;
        return this;
    }

    public ItemSelectorBoxBuilder<T> OnRemove(Action<T> onRemove)
    {
        _onRemove = onRemove;
        return this;
    }

    public ItemSelectorBoxBuilder<T> OnSelect(Action<T> onSelect)
    {
        _onSelect = onSelect;
        return this;
    }

    public ItemSelectorBox<T> Build()
        => new ItemSelectorBox<T>(_addIcon, _addIconText, _colSelected, 
            _colHovered, _bgCol, _onSelect, _onRemove, _onRename, _onAdd);
}


/// <summary> 
///     A generic widget format for the <seealso cref="TagCollection"/>, that has it's own builder.
/// </summary>
public class ItemSelectorBox<T>
{
    public const string HELP_MAIN = "Keybinds:--SEP--";
    public const string LONGHELP_SHIFTLEFT = "--NL----COL--[SHIFT + L-Click]--COL-- ⇒ Shift entry to the left.";
    public const string LONGHELP_SHIFTRIGHT = "--NL----COL--[SHIFT + R-Click]--COL-- ⇒ Shift entry to the right.";
    public const string LONGHELP_SELECT = "--NL----COL--[L-Click]--COL-- ⇒ Select entry.";
    public const string LONGHELP_RENAME = "--NL----COL--[R-Click]--COL-- ⇒ Rename entry.";
    public const string LONGHELP_REMOVE = "--NL----COL--[CTRL + SHIFT + R-Click]--COL-- ⇒ Remove entry.";

    public const string SHORTHELP_SHIFTLEFT = "--COL--[SHIFT + L-Click]--COL-- ⇒ Shift left";
    public const string SHORTHELP_SHIFTRIGHT = "--NL----COL--[SHIFT + R-Click]--COL-- ⇒ Shift right";
    public const string SHORTHELP_SELECT = "--NL----COL--[L-Click]--COL-- ⇒ Select";
    public const string SHORTHELP_RENAME = "--NL----COL--[R-Click]--COL-- ⇒ Rename";
    public const string SHORTHELP_REMOVE = "--NL----COL--[CTRL + SHIFT + R-Click]--COL-- ⇒ Remove";

    private readonly FAI _addItemIcon;
    private readonly string _addItemText;
    private readonly uint _colSelected;
    private readonly uint _colHovered;
    private readonly uint _bgCol;

    private string _helpStringLong;
    private string _helpStringShort;
    private int _renameIdx = -1;
    private bool _setFocus;
    private string _currentName = string.Empty;

    private readonly Action<T>? OnSelect;
    private readonly Action<T>? OnRemove;
    private readonly Action<T, string>? OnRename;
    private readonly Action<string>? OnAdd;
    private readonly bool _allowShift;
    public ItemSelectorBox(
        FAI addIcon,
        string addTxt,
        uint? selectedCol = null,
        uint? hoveredCol = null,
        uint? bgCol = null,
        Action<T>? onSelected = null, 
        Action<T>? onRemove = null,
        Action<T, string>? onRename = null,
        Action<string>? onAdd = null,
        bool allowShift = false)
    {
        _addItemIcon = addIcon;
        _addItemText = addTxt;
        _colSelected = selectedCol ?? CkColor.VibrantPink.Uint();
        _colHovered = hoveredCol ?? ImGui.GetColorU32(ImGuiCol.FrameBgHovered);
        _bgCol = bgCol ?? CkColor.FancyHeaderContrast.Uint();

        _allowShift = allowShift;
        OnAdd = onAdd;

        // add the elements.
        var longHelp = new StringBuilder(HELP_MAIN);
        var shortHelp = new StringBuilder(HELP_MAIN);

        if (onSelected is not null)
        {
            OnSelect = onSelected;
            longHelp.Append(LONGHELP_SELECT);
            shortHelp.Append(SHORTHELP_SELECT);
        }
        if (onRename is not null)
        {
            OnRename = onRename;
            longHelp.Append(LONGHELP_RENAME);
            shortHelp.Append(SHORTHELP_RENAME);
        }
        if (allowShift)
        {
            OnAdd = onAdd;
            longHelp.Append(LONGHELP_SHIFTLEFT);
            shortHelp.Append(SHORTHELP_SHIFTLEFT);
            longHelp.Append(LONGHELP_SHIFTRIGHT);
            shortHelp.Append(SHORTHELP_SHIFTRIGHT);
        }
        if (onRemove is not null)
        {
            OnRemove = onRemove;
            longHelp.Append(LONGHELP_REMOVE);
            shortHelp.Append(SHORTHELP_REMOVE);
        }
        _helpStringLong = longHelp.ToString();
        _helpStringShort = shortHelp.ToString();
    }

    /// <summary>
    ///     The primary draw function. <para />
    ///     TODO: Make this for IEnumerable<typeparamref name="T"/> or IReadOnlyCollection<typeparamref name="T"/> instead.
    ///     This is currenly list so that changes to its order are performed on the passed in list.
    /// </summary>
    public void DrawSelectorChildBox(string id, Vector2 region, bool lockFirst, IReadOnlyCollection<T> items, T? selected, Func<T, string> toName)
    {
        using var _ = ImRaii.PushId(id);
        using var child = CkRaii.Child(id, region, wFlags: WFlags.NoScrollbar);

        var color = ImGui.GetColorU32(ImGuiCol.Button);
        using var s = ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, ImGui.GetStyle().ItemSpacing with { X = 4 * ImGuiHelpers.GlobalScale });
        using var c = ImRaii.PushColor(ImGuiCol.ButtonHovered, _colHovered)
            .Push(ImGuiCol.ButtonActive, ColorHelpers.Lighten(_bgCol, .25f))
            .Push(ImGuiCol.Button, _bgCol);

        var x = ImGui.GetCursorPosX();
        ImGui.SetCursorPosX(x);
        var rightEndOffset = 4 * ImGuiHelpers.GlobalScale;

        DrawSelectorInternal(x, rightEndOffset, lockFirst, items, selected, toName);
    }

    /// <summary>
    ///     The primary draw function. <para />
    ///     TODO: Make this for IEnumerable<typeparamref name="T"/> or IReadOnlyCollection<typeparamref name="T"/> instead.
    ///     This is currenly list so that changes to its order are performed on the passed in list.
    /// </summary>
    public void DrawSelectorBox(string id, bool lockFirst, IReadOnlyCollection<T> items, T? selected, Func<T, string> toName)
    {
        using var _ = ImRaii.PushId(id);
        using var group = ImRaii.Group();

        using var s = ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, ImGui.GetStyle().ItemSpacing with { X = 4 * ImGuiHelpers.GlobalScale });
        using var c = ImRaii.PushColor(ImGuiCol.ButtonHovered, _colHovered)
            .Push(ImGuiCol.ButtonActive, ColorHelpers.Lighten(_bgCol, .25f))
            .Push(ImGuiCol.Button, _bgCol);

        var x = ImGui.GetCursorPosX();
        ImGui.SetCursorPosX(x);
        var rightEndOffset = 4 * ImGuiHelpers.GlobalScale;

        DrawSelectorInternal(x, rightEndOffset, lockFirst, items, selected, toName);
    }

    //public bool DrawHelpButtons(Vector2 botRight, string csvString, out string updatedCsvString, bool showSort)
    //{
    //    bool change = false;

    //    // Update the tags.
    //    UpdateOrSetLatest(csvString);

    //    // do the draws.
    //    var iconSize = new Vector2(ImGui.GetFrameHeight());
    //    var spacingX = ImGui.GetStyle().ItemInnerSpacing.X * .5f;
    //    float width = showSort ? iconSize.X * 2 + spacingX : iconSize.X;
    //    var iconsSize = new Vector2(width, iconSize.Y);
    //    // Move and draw the icons.
    //    var newPos = botRight - iconsSize;
    //    ImGui.SetCursorScreenPos(newPos);
    //    if (showSort)
    //    {
    //        var hovered = ImGui.IsMouseHoveringRect(newPos, newPos + iconSize);
    //        CkGui.FramedIconText(FontAwesomeIcon.SortAlphaDown, ImGui.GetColorU32(hovered ? ImGuiCol.ButtonHovered : ImGuiCol.Text));
    //        if (ImGui.IsItemClicked(ImGuiMouseButton.Left) && hovered)
    //        {
    //            items.Sort();
    //            change = true;
    //        }
    //        CkGui.AttachToolTip("Sort Tags Alphabetically");
    //        ImGui.SameLine();
    //    }
    //    CkGui.HelpText(HELP_TEXT, CkColor.VibrantPink.Uint());

    //    updatedCsvString = string.Join(", ", items);
    //    return change;
    //}

    private bool DrawSelectorInternal(float x, float rightEndOffset, bool lockFirst, IReadOnlyCollection<T> items, T? selected, Func<T, string> toName)
    {
        var changeOccurred = false;
        var wdl = ImGui.GetWindowDrawList();
        var spacing = ImGui.GetStyle().ItemSpacing.X;
        var padding = ImGui.GetStyle().FramePadding.X;
        var itemsList = items.ToList();
        foreach ((var item, var idx) in itemsList.WithIndex())
        {
            using var _ = ImRaii.PushId(idx);
            var itemName = toName(item);
            if (_renameIdx == idx)
            {
                var width = SetPosText(_currentName, x);
                SetFocus();

                ImGui.SetNextItemWidth(width);
                ImGui.InputText("##edit", ref _currentName, 128);
                if (ImGui.IsItemDeactivated())
                {
                    _renameIdx = -1;
                    if (_currentName != itemName)
                    {
                        OnRename?.Invoke(item, _currentName);
                        changeOccurred = OnRename is not null;
                    }
                }
            }
            else
            {
                SetPosButton(itemName, x, rightEndOffset);
                Button(item, itemName, idx, item?.Equals(selected) ?? false);
            }

            ImGui.SameLine();
        }

        if (_renameIdx == items.Count)
        {
            var width = SetPosText(_currentName, x);
            SetFocus();

            ImGui.SetNextItemWidth(width);
            var textChanged = ImGui.InputText($"##addEdit{_renameIdx}", ref _currentName, 128);
            var tabPressed = ImGui.IsItemActive() && ImGui.IsKeyPressed(ImGuiKey.Tab, false);

            // Handle TAB press immediately, not on deactivation
            if (tabPressed)
            {
                if (!string.IsNullOrWhiteSpace(_currentName))
                {
                    OnAdd?.Invoke(_currentName);
                    changeOccurred = OnAdd is not null;
                }
                // Stay in add mode for next entry
                _renameIdx = items.Count;
                _setFocus = true;
                _currentName = string.Empty;
                return changeOccurred;
            }

            // Handle deactivation (e.g., when clicking outside or pressing Enter)
            if (ImGui.IsItemDeactivated())
            {
                if (string.IsNullOrWhiteSpace(_currentName))
                {
                    _renameIdx = -1;
                    _currentName = string.Empty;
                    changeOccurred = false;
                }
                else
                {
                    OnAdd?.Invoke(_currentName);
                    _renameIdx = -1;
                    changeOccurred = OnAdd is not null;
                }
            }
        }
        else
        {
            var newButtonWidth = _addItemText.Length > 0
                ? CkGui.IconButtonSize(_addItemIcon).X 
                : CkGui.IconTextButtonSize(_addItemIcon, _addItemText);

            SetPos(newButtonWidth, x, rightEndOffset);
            if (_addItemText.Length > 0)
            {
                if (CkGui.IconTextButton(_addItemIcon, _addItemText, isInPopup: true))
                {
                    _renameIdx = items.Count;
                    _setFocus = true;
                    _currentName = string.Empty;
                }
            }
            else
            {
                if (CkGui.IconButton(_addItemIcon, inPopup: true))
                {
                    _renameIdx = items.Count;
                    _setFocus = true;
                    _currentName = string.Empty;
                }   
            }
            CkGui.AttachToolTip("Add Item...");
        }

        return changeOccurred;

        void Button(T item, string itemName, int idx, bool selected)
        {
            var pressed = ImGui.Button(itemName);
            var lClicked = ImGui.IsItemClicked(ImGuiMouseButton.Left);
            var rClicked = ImGui.IsItemClicked(ImGuiMouseButton.Right);
            wdl.AddRect(ImGui.GetItemRectMin(), ImGui.GetItemRectMax(), selected ? _colSelected : _bgCol, padding, ImDrawFlags.RoundCornersAll, CkStyle.ThinThickness());
            CkGui.AttachToolTip(_helpStringShort, color: CkColor.VibrantPink.Vec4());

            // Rearrangement.
            if (lClicked)
            {
                // handle shifting, if allowed.
                if (KeyMonitor.ShiftPressed() && _allowShift && idx > 0)
                {
                    if (idx is 0 && lockFirst)
                        return;

                    (itemsList[idx], itemsList[idx - 1]) = (itemsList[idx - 1], itemsList[idx]);
                    changeOccurred = true;
                }
                // handle normal click, which is selection.
                else
                {
                    OnSelect?.Invoke(item);
                    changeOccurred = OnSelect is not null;
                }
            }
            else if (rClicked)
            {
                if (idx is 0 && lockFirst)
                    return;

                // handle removal, if CTRL + SHIFT is pressed.
                if (KeyMonitor.ShiftPressed())
                {
                    if (KeyMonitor.CtrlPressed())
                    {
                        OnRemove?.Invoke(item);
                        changeOccurred = OnRemove is not null;
                    }
                    else if (idx < items.Count - 1)
                    {
                        (itemsList[idx], itemsList[idx + 1]) = (itemsList[idx + 1], itemsList[idx]);
                        changeOccurred = true;
                    }
                }
                // Normal rename.
                else
                {
                    _renameIdx = idx;
                    _setFocus = true;
                    _currentName = itemName;
                }
            }
        }

        float SetPosButton(string itemName, float x, float rightEndOffset = 0)
            => SetPos(ImGui.CalcTextSize(itemName).X + padding * 2, x, rightEndOffset);
        
        float SetPosText(string itemName, float x)
            => SetPos(ImGui.CalcTextSize(itemName).X + padding * 2 + 15 * ImGuiHelpers.GlobalScale, x);


        float SetPos(float width, float x, float rightEndOffset = 0)
        {
            if (width + spacing >= ImGui.GetContentRegionAvail().X - rightEndOffset)
            {
                ImGui.NewLine();
                ImGui.SetCursorPosX(x);
            }

            return width;
        }
    }

    private void SetFocus()
    {
        if (!_setFocus)
            return;

        ImGui.SetKeyboardFocusHere();
        _setFocus = false;
    }
}
