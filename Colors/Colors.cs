using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using static FFXIVClientStructs.STD.Helper.IStaticMemorySpace;

namespace CkCommons;

/// <summary>
///     Colors used for CkCommons Internal style elements.
/// </summary>
[Flags]
public enum CkCol : int
{
    // General Colors
    Help, // Unused ATM
    HelpHovered,
    HelpActive,
    HelpDisabled, // Unused ATM

    ProgressBar, // Unused?

    BoxItem,
    BoxItemHovered,
    BoxItemActive,
    BoxItemDisabled,
    BoxItemFrame,

    // Tooltips
    TipText,
    TipFrame,

    // CkGui.ColorSeperator
    Divider,
    DividerHovered,
    DividerActive,

    // Favoriting
    Favorite,
    FavoriteHovered,
    FavoriteActive,
    FavoriteOff,

    // TriStateBoxes
    TriStateCheck,
    TriStateCross,
    TriStateNeutral,

    // IconCheckboxes (Same as above? idk)
    IconOn,
    IconOff,

    // File System
    FsFolderOpen,
    FsFolderClose,
    FsFolderLine,

    // Draw System
    DdsGroupIcon,
    DdsGroupBg,
    DdsGroupBorder,
    DdsGroupGradient,

    DdsFolderIcon,
    DdsFolderBg,
    DdsFolderBorder,
    DdsFolderGradient,

    DdsNodeLine,
    DdsLeafBg,

    // CkRaii.Child & Variants
    ChildFrame,

    // CkRaii.HeaderChild
    HChild,
    HChildHovered,
    HChildActive,
    HChildDisabled,
    HChildSplit,
    HChildBg,
    HChildFrame,

    // CkRaii.LabeledChilds
    LChild,
    LChildHovered,
    LChildActive,
    LChildDisabled,
    LChildSplit,
    LChildBg,
    LChildFrame,

    // CkRaii.FancyHeader
    CurvedHeader,
    CurvedHeaderFade,

    // CKRaii.FancySearch
    SearchBg,
    SearchFrame,

    // CkRaii.FancyTabBar
    TabBar,
    TabHovered,
    TabActive,
    TabDisabled,
    TabBarSplit,
    TabBarFrame,

    /// <summary>
    ///     The total number of ColorVars in CkCommons
    /// </summary>
    Count,
}


internal struct ColorMod
{
    public CkCol Var;
    public Vector4 Backup;
}

public static class CkColors
{
    public static readonly int          Count = Enum.GetValues<CkCol>().Length - 1;
    // Default Colors (Persistant)
    private static readonly Vector4[]   BaseColors = new Vector4[Count];
    // Current Colors (Read by Widgets)
    private static readonly Vector4[]   CurrentColors = new Vector4[Count];
    // Need a ColorMod for the temporary overrides
    private static readonly ColorMod[]  Stack = new ColorMod[256];
    private static int StackTop;

    // Static constructor to set the base values on initialization
    static CkColors()
    {
        for (int i = 0; i < Count; i++)
            BaseColors[i] = GetDefault((CkCol)i);
        ApplyTheme();
    }

    // Casts
    public static Dictionary<CkCol, Vector4> AsVec4Dictionary()
        => Enumerable.Range(0, Count).ToDictionary(i => (CkCol)i, i => CurrentColors[i]);

    public static Dictionary<CkCol, uint> AsUintDictionary()
        => Enumerable.Range(0, Count).ToDictionary(i => (CkCol)i, i => CurrentColors[i].ToUint());


    // Accessors from frequently used paths
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref Vector4 Vec4Ref(this CkCol var)
        => ref CurrentColors[(int)var];

    /// <summary>
    ///     Gets the <see cref="Vector4"/> value of the <see cref="CkCol"/>
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector4 Vec4(this CkCol var)
        => CurrentColors[(int)var];

    /// <summary>
    ///     Gets the <see cref="uint"/> value of the <see cref="CkCol"/>
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint Uint(this CkCol var)
        => CurrentColors[(int)var].ToUint();

    #region Themes
    /// <summary>
    ///     Applies a preset theme to CkCommons colors
    /// </summary>
    public static void SetTheme(Dictionary<CkCol, uint>? theme = null)
    {
        for (int i = 0; i < Count; i++)
        {
            if (theme != null && theme.TryGetValue((CkCol)i, out var col))
                BaseColors[i] = col.ToVec4();
            else
                BaseColors[i] = GetDefault((CkCol)i); // fallback to built-in defaults
        }
        // Apply it
        ApplyTheme();
    }

    /// <summary>
    ///     Only modifies the base theme, does NOT touch active overrides
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetDefault(CkCol var, Vector4 color)
        => BaseColors[(int)var] = color;
    
    /// <summary>
    ///     Applies the changes from the base colors over to the current colors, and drops the stacktop.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ApplyTheme()
    {
        Array.Copy(BaseColors, CurrentColors, Count);
        StackTop = 0; // Reset stack
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void RevertCol(CkCol col)
    {
        BaseColors[(int)col] = GetDefault(col);
        ApplyTheme(); // This feels unstable, idk...
    }

    /// <summary>
    ///     Reset the applied theme to the default colors
    /// </summary>
    public static void RevertAll()
    {
        Debug.Assert(StackTop == 0, "Cannot reset theme while colors are pushed.");
        for (int i = 0; i < Count; i++)
            BaseColors[i] = GetDefault((CkCol)i);
        ApplyTheme();
    }

    #endregion Themes

    #region Push & Pop

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void PushColor(CkCol var, Vector4 color)
    {
        Debug.Assert(StackTop < Stack.Length, "Stack overflow in PushColor");
        ref var slot = ref CurrentColors[(int)var];
        Stack[StackTop++] = new ColorMod { Var = var, Backup = slot };
        slot = color;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void PushColor(CkCol var, uint color)
        => PushColor(var, color.ToVec4());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void PopColor(int count)
    {
        // Maybe less assert and more graceful handling on pops by allowing count to exceed size so long as it reverts all?
        Debug.Assert(StackTop >= count, "Stack underflow in PopColor");
        while (count-- > 0)
        {
            var mod = Stack[--StackTop];
            CurrentColors[(int)mod.Var] = mod.Backup;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void PopColor()
        => PopColor(1);

    #endregion Push & Pop

    #region Conversions

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint ToUint(this Vector4 color)
    {
        var r = (byte)(color.X * 255);
        var g = (byte)(color.Y * 255);
        var b = (byte)(color.Z * 255);
        var a = (byte)(color.W * 255);
        return (uint)((a << 24) | (b << 16) | (g << 8) | r);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector4 ToVec4(this uint color)
    {
        var r = (color & 0x000000FF) / 255f;
        var g = ((color & 0x0000FF00) >> 8) / 255f;
        var b = ((color & 0x00FF0000) >> 16) / 255f;
        var a = ((color & 0xFF000000) >> 24) / 255f;
        return new Vector4(r, g, b, a);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToHexString(this uint color)
        => $"0x{color:X8}";
    #endregion Conversions

    /// <summary>
    ///     The Default CkColors. <para />
    ///     List is extensive, and can likely optimize to trim down.
    ///     But worry about it later.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector4 GetDefault(CkCol color)
        => color switch
        {
            // General Colors
            CkCol.Help              => new Vector4(0.500f, 0.500f, 0.500f, 1.000f),
            CkCol.HelpHovered       => new Vector4(0.000f, 0.600f, 1.000f, 1.000f),
            CkCol.HelpActive        => new Vector4(0.000f, 0.800f, 1.000f, 1.000f),
            CkCol.HelpDisabled      => new Vector4(0.350f, 0.350f, 0.350f, 1.000f),

            CkCol.ProgressBar       => new Vector4(0.977f, 0.380f, 0.640f, 0.914f), // Placeholder

            CkCol.BoxItem           => new Vector4(0.000f, 0.000f, 0.000f, 0.000f), // ADD_LATER
            CkCol.BoxItemHovered    => new Vector4(0.000f, 0.000f, 0.000f, 0.000f), // ADD_LATER
            CkCol.BoxItemActive     => new Vector4(0.000f, 0.000f, 0.000f, 0.000f), // ADD_LATER
            CkCol.BoxItemDisabled   => new Vector4(0.000f, 0.000f, 0.000f, 0.000f), // ADD_LATER
            CkCol.BoxItemFrame      => new Vector4(0.000f, 0.000f, 0.000f, 0.000f), // ADD_LATER

            // Tooltips
            CkCol.TipText           => new Vector4(0.977f, 0.380f, 0.640f, 0.914f), // Placeholder Line
            CkCol.TipFrame          => new Vector4(0.977f, 0.380f, 0.640f, 0.914f),

            // CkGui.ColorSeperator
            CkCol.Divider           => new Vector4(0.000f, 0.000f, 0.000f, 0.000f), // ADD_LATER
            CkCol.DividerHovered    => new Vector4(0.000f, 0.000f, 0.000f, 0.000f), // ADD_LATER
            CkCol.DividerActive     => new Vector4(0.000f, 0.000f, 0.000f, 0.000f), // ADD_LATER

            // Favorite Star Utils
            CkCol.Favorite          => new Vector4(0.816f, 0.816f, 0.251f, 1.000f),
            CkCol.FavoriteHovered   => new Vector4(0.816f, 0.251f, 0.816f, 1.000f),
            CkCol.FavoriteOff       => new Vector4(0.502f, 0.502f, 0.502f, 0.125f),

            // TriStateBoxes
            CkCol.TriStateCheck     => new Vector4(0.000f, 0.816f, 0.000f, 1.000f),
            CkCol.TriStateCross     => new Vector4(0.816f, 0.000f, 0.000f, 1.000f),
            CkCol.TriStateNeutral   => new Vector4(0.816f, 0.816f, 0.816f, 1.000f),

            // Other Boxes
            CkCol.IconOn            => new Vector4(0.000f, 0.816f, 0.000f, 1.000f),
            CkCol.IconOff           => new Vector4(0.816f, 0.816f, 0.816f, 1.000f),

            // File System
            CkCol.FsFolderOpen      => new Vector4(1.000f, 1.000f, 1.000f, 1.000f),
            CkCol.FsFolderClose     => new Vector4(1.000f, 1.000f, 1.000f, 1.000f),
            CkCol.FsFolderLine      => new Vector4(1.000f, 1.000f, 1.000f, 1.000f),

            // DrawSystem
            CkCol.DdsGroupIcon      => new Vector4(1.000f, 1.000f, 1.000f, 1.000f),
            CkCol.DdsGroupBg        => new Vector4(1.000f, 1.000f, 1.000f, 1.000f),
            CkCol.DdsGroupBorder    => new Vector4(1.000f, 1.000f, 1.000f, 1.000f),
            CkCol.DdsGroupGradient  => new Vector4(0.500f, 0.500f, 0.500f, 1.000f), // Multiplied by fade intensity?
            
            // Maybe merge these two groups into one set of "DrawSystemFolder" colors?
            CkCol.DdsFolderIcon     => new Vector4(1.000f, 1.000f, 1.000f, 1.000f),
            CkCol.DdsFolderBg       => new Vector4(1.000f, 1.000f, 1.000f, 1.000f),
            CkCol.DdsFolderBorder   => new Vector4(1.000f, 1.000f, 1.000f, 1.000f),
            CkCol.DdsFolderGradient => new Vector4(0.500f, 0.500f, 0.500f, 1.000f), // Multiplied by fade intensity?

            CkCol.DdsNodeLine       => new Vector4(1.000f, 1.000f, 1.000f, 1.000f),
            CkCol.DdsLeafBg         => new Vector4(0.050f, 0.050f, 0.050f, 1.000f),

            // CkRaii.Child & Variants
            CkCol.ChildFrame        => new Vector4(0.806f, 0.102f, 0.407f, 1.000f), // Placeholder

            // CkRaii.HeaderChild
            CkCol.HChild            => new Vector4(1.000f, 0.181f, 0.715f, 0.825f),
            CkCol.HChildHovered     => new Vector4(0.000f, 0.000f, 0.000f, 0.000f), // ADD_LATER
            CkCol.HChildActive      => new Vector4(0.000f, 0.000f, 0.000f, 0.000f), // ADD_LATER
            CkCol.HChildDisabled    => new Vector4(0.000f, 0.000f, 0.000f, 0.000f), // ADD_LATER
            CkCol.HChildSplit       => new Vector4(0.180f, 0.180f, 0.180f, 1.000f),
            CkCol.HChildBg          => new Vector4(1.000f, 0.742f, 0.910f, 0.416f),
            CkCol.HChildFrame       => new Vector4(0.000f, 0.000f, 0.000f, 0.000f), // ADD_LATER_MAYBE

            // CkRaii.LabeledChilds
            CkCol.LChild            => new Vector4(0.977f, 0.380f, 0.640f, 0.914f), // Placeholder (GsCol Pink)
            CkCol.LChildHovered     => new Vector4(0.986f, 0.464f, 0.691f, 0.955f), // Placeholder (GsCol PinkHovered)
            CkCol.LChildActive      => new Vector4(0.846f, 0.276f, 0.523f, 0.769f), // Placeholder (GsCol PinkActive)
            CkCol.LChildDisabled    => new Vector4(0.000f, 0.000f, 0.000f, 0.000f), // ADD_LATER
            CkCol.LChildSplit       => new Vector4(0.180f, 0.180f, 0.180f, 1.000f), // Placeholder (HChildSplit)
            CkCol.LChildBg          => new Vector4(0.579f, 0.170f, 0.359f, 0.828f), // Placeholder (GsCol PinkHovered)
            CkCol.LChildFrame       => new Vector4(0.000f, 0.000f, 0.000f, 0.000f), // ADD_LATER

            // CkRaii.CurvedHeader
            CkCol.CurvedHeader      => new Vector4(0.579f, 0.170f, 0.359f, 0.828f),
            CkCol.CurvedHeaderFade  => new Vector4(0.100f, 0.022f, 0.022f, 0.299f),

            // CKRaii.FancySearch
            CkCol.SearchBg          => new Vector4(0.290f, 0.290f, 0.290f, 0.540f),
            CkCol.SearchFrame       => new Vector4(0.180f, 0.180f, 0.180f, 1.000f), // TUNE_LATER

            // CkRaii.FancyTabBar
            CkCol.TabBar            => new Vector4(0.579f, 0.170f, 0.359f, 0.828f), // Placeholder (CurvedHeader)
            CkCol.TabHovered        => new Vector4(0.986f, 0.464f, 0.691f, 0.955f), // Placeholder (GsCol PinkHovered)
            CkCol.TabActive         => new Vector4(0.977f, 0.380f, 0.640f, 0.914f), // Placeholder (GsCol Pink)
            CkCol.TabDisabled       => new Vector4(0.000f, 0.000f, 0.000f, 0.000f), // DARK_PINK_ADD_LATER
            CkCol.TabBarSplit       => new Vector4(0.000f, 0.000f, 0.000f, 0.000f), // ADD_LATER
            CkCol.TabBarFrame       => new Vector4(0.579f, 0.170f, 0.359f, 0.828f), // Placeholder (CurvedHeader)

            _ => Vector4.Zero,
        };

    public static string ToName(this CkCol idx) => idx switch
    {
        // General
        CkCol.Help => "Help",
        CkCol.HelpHovered => "Help (Hovered)",
        CkCol.HelpActive => "Help (Active)",
        CkCol.HelpDisabled => "Help (Disabled)",

        CkCol.ProgressBar => "Progress Bar",

        CkCol.BoxItem => "Box Item",
        CkCol.BoxItemHovered => "Box Item (Hovered)",
        CkCol.BoxItemActive => "Box Item (Active)",
        CkCol.BoxItemDisabled => "Box Item (Disabled)",
        CkCol.BoxItemFrame => "Box Item Frame",

        // Tooltips
        CkCol.TipText => "Tooltip Text (Colored)",
        CkCol.TipFrame => "Tooltip Frame",

        // Dividers / separators
        CkCol.Divider => "Divider",
        CkCol.DividerHovered => "Divider (Hovered)",
        CkCol.DividerActive => "Divider (Active)",

        // Favoriting
        CkCol.Favorite => "Favorite",
        CkCol.FavoriteHovered => "Favorite (Hovered)",
        CkCol.FavoriteActive => "Favorite (Active)",
        CkCol.FavoriteOff => "Favorite (Off)",

        // TriStateBoxes
        CkCol.TriStateCheck => "TriState Check",
        CkCol.TriStateCross => "TriState Cross",
        CkCol.TriStateNeutral => "TriState Neutral",

        // IconCheckboxes
        CkCol.IconOn => "Icon On",
        CkCol.IconOff => "Icon Off",

        // File System
        CkCol.FsFolderOpen => "FS Folder Open",
        CkCol.FsFolderClose => "FS Folder Close",
        CkCol.FsFolderLine => "FS Folder Line",

        // Draw System
        CkCol.DdsGroupIcon => "DDS Group Icon",
        CkCol.DdsGroupBg => "DDS Group BG",
        CkCol.DdsGroupBorder => "DDS Group Border",
        CkCol.DdsGroupGradient => "DDS Group Gradient",

        CkCol.DdsFolderIcon => "DDS Folder Icon",
        CkCol.DdsFolderBg => "DDS Folder Background",
        CkCol.DdsFolderBorder => "DDS Folder Border",
        CkCol.DdsFolderGradient => "DDS Folder Gradient",

        CkCol.DdsNodeLine => "DDS Node Line",
        CkCol.DdsLeafBg => "DDS Leaf BG",

        // CkRaii.Child & Variants
        CkCol.ChildFrame => "Child Frame",

        // CkRaii.HeaderChild
        CkCol.HChild => "Header Child",
        CkCol.HChildHovered => "Header Child (Hovered)",
        CkCol.HChildActive => "Header Child (Active)",
        CkCol.HChildDisabled => "Header Child (Disabled)",
        CkCol.HChildSplit => "Header Child Split",
        CkCol.HChildBg => "Header Child BG",
        CkCol.HChildFrame => "Header Child Frame",

        // CkRaii.LabeledChilds
        CkCol.LChild => "Labeled Child",
        CkCol.LChildHovered => "Labeled Child (Hovered)",
        CkCol.LChildActive => "Labeled Child (Active)",
        CkCol.LChildDisabled => "Labeled Child (Disabled)",
        CkCol.LChildSplit => "Labeled Child Split",
        CkCol.LChildBg => "Labeled Child BG",
        CkCol.LChildFrame => "Labeled Child Frame",

        // CkRaii.FancyHeader
        CkCol.CurvedHeader => "Fancy Header",
        CkCol.CurvedHeaderFade => "Fancy Header Shadow",

        // CkRaii.FancyTabBar
        CkCol.TabBar => "Tab Bar",
        CkCol.TabHovered => "Tab (Hovered)",
        CkCol.TabActive => "Tab (Active)",
        CkCol.TabDisabled => "Tab (Disabled)",
        CkCol.TabBarSplit => "Tab Bar Split",

        // Fallback
        _ => idx.ToString()
    };

    public static void Vec4ToClipboard(Dictionary<CkCol, Vector4> cols)
    {
        if (cols is null || cols.Count is 0)
            return;

        var sb = new StringBuilder();
        sb.AppendLine($"public static readonly Dictionary<CkCol, Vector4> TEMPLATE = new Dictionary<CkCol, Vector4>");
        sb.AppendLine("{");

        var maxEnumLen = cols.Keys.Max(k => k.ToString().Length);
        foreach (var kvp in cols.OrderBy(k => (int)k.Key))
        {
            var name = kvp.Key.ToString().PadRight(maxEnumLen);
            var v = kvp.Value;
            sb.AppendLine($"    {{ CkCol.{name}, new Vector4({v.X:0.###}f, {v.Y:0.###}f, {v.Z:0.###}f, {v.W:0.###}f) }},");
        }
        sb.AppendLine("};");

        Clipboard.SetText(sb.ToString());
    }

    public static void UintToClipboard(Dictionary<CkCol, uint> cols)
    {
        if (cols is null || cols.Count is 0)
            return;

        var sb = new StringBuilder();
        sb.AppendLine($"public static readonly IReadOnlyDictionary<CkCol, uint> TEMPLATE = new Dictionary<CkCol, uint>");
        sb.AppendLine("{");
        
        var maxEnumLen = cols.Keys.Max(k => k.ToString().Length);
        foreach (var kvp in cols.OrderBy(k => (int)k.Key))
            sb.AppendLine($"    {{ CkCol.{kvp.Key.ToString().PadRight(maxEnumLen)}, 0x{kvp.Value:X8} }},");
        sb.AppendLine("};");

        Clipboard.SetText(sb.ToString());
    }
}
