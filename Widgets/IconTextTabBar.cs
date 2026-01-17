using CkCommons.Gui;
using Dalamud.Bindings.ImGui;

namespace CkCommons.Widgets;

public abstract class IconTextTabBar<ITab> where ITab : Enum
{
    protected record TabButtonDefinition(FAI Icon, string Text, ITab TargetTab, string Tooltip, Action? CustomAct = null);

    protected readonly List<TabButtonDefinition> _tabButtons = new();
    private ITab _selectedTab;
    public virtual ITab TabSelection
    {
        get => _selectedTab;
        set
        {
            TabSelectionChanged?.Invoke(_selectedTab, value);
            _selectedTab = value;
        }
    }

    protected IconTextTabBar()
    {
        _selectedTab = default!; // Assuming the default value of ITab is a valid tab.
    }

    /// <summary> Invokes actions informing people of the previous and new tab selected. </summary>
    public event Action<ITab, ITab>? TabSelectionChanged;
    protected virtual bool IsTabDisabled(ITab tab) => false;

    public void AddDrawButton(FAI icon, string text, ITab targetTab, string tooltip, Action? customAct = null)
    {
        _tabButtons.Add(new TabButtonDefinition(icon, text, targetTab, tooltip, customAct));
    }

    protected virtual void DrawTabButton(TabButtonDefinition tab, Vector2 buttonSize, Vector2 spacing, ImDrawListPtr drawList)
    {
        var x = ImGui.GetCursorScreenPos();

        var isDisabled = IsTabDisabled(tab.TargetTab);

        if (CkGui.IconTextButtonCentered(tab.Icon, tab.Text, buttonSize.X, true, isDisabled))
            TabSelection = tab.TargetTab;
        CkGui.AttachToolTip(tab.Tooltip);

        ImGui.SameLine();
        var xPost = ImGui.GetCursorScreenPos();
        if (EqualityComparer<ITab>.Default.Equals(TabSelection, tab.TargetTab))
        {
            drawList.AddLine(
                x with { Y = x.Y + buttonSize.Y + spacing.Y },
                xPost with { Y = xPost.Y + buttonSize.Y + spacing.Y, X = xPost.X - spacing.X },
                ImGui.GetColorU32(ImGuiCol.Separator), 2f);
        }

        // invoke action if we should.
        tab.CustomAct?.Invoke();
    }

    public abstract void Draw(float widthAvailable);
}

