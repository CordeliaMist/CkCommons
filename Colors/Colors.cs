using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Runtime.CompilerServices;

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
    TooltipFrame,

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
    LChildLabel,
    LChildLabelHovered, // Maybe remove idk
    LChild,
    LChildHovered,
    LChildActive,
    LChildDisabled,
    LChildSplit,
    LChildBg,
    LChildFrame,

    // CkRaii.FancyHeader
    CHeader,
    CHeaderShadow,

    // CkRaii.FancyTabBar
    TabBar,
    TabHovered,
    TabActive,
    TabDisabled,
    TabBarSplit,

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
    private static readonly int ColorCount = Enum.GetValues<CkCol>().Length - 1;

    // Default Colors (Persistant)
    private static readonly Vector4[] BaseColors = new Vector4[ColorCount];

    // Current Colors (Read by Widgets)
    private static readonly Vector4[] CurrentColors = new Vector4[ColorCount];

    // Need a ColorMod for the temporary overrides
    private static readonly ColorMod[] Stack = new ColorMod[256];
    private static int StackTop;

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
        for (int i = 0; i < ColorCount; i++)
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
        Array.Copy(BaseColors, CurrentColors, ColorCount);
        StackTop = 0; // Reset stack
    }


    /// <summary>
    ///     Reset the applied theme to the default colors
    /// </summary>
    public static void ResetThemeToDefaults()
    {
        Debug.Assert(StackTop == 0, "Cannot reset theme while colors are pushed.");
        for (int i = 0; i < ColorCount; i++)
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

    #endregion Conversions

    /// <summary>
    ///     The Default CkColors
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector4 GetDefault(CkCol color)
        => color switch
        {
            CkCol.CHeader           => new Vector4(0.579f, 0.170f, 0.359f, 0.828f),
            CkCol.CHeaderShadow     => new Vector4(0.100f, 0.022f, 0.022f, 0.299f),
            CkCol.HChild            => new Vector4(1.000f, 0.181f, 0.715f, 0.825f),
            CkCol.HChildSplit       => new Vector4(0.180f, 0.180f, 0.180f, 1.000f),
            CkCol.HChildBg          => new Vector4(1.000f, 0.742f, 0.910f, 0.416f),

            CkCol.FsFolderOpen      => new Vector4(0.753f, 0.941f, 1.000f, 1.000f),
            CkCol.FsFolderClose     => new Vector4(0.753f, 0.941f, 1.000f, 1.000f),
            CkCol.FsFolderLine      => new Vector4(0.753f, 0.941f, 1.000f, 1.000f),

            CkCol.Favorite          => new Vector4(0.816f, 0.816f, 0.251f, 1.000f),
            CkCol.FavoriteHovered   => new Vector4(0.816f, 0.251f, 0.816f, 1.000f),
            CkCol.FavoriteOff       => new Vector4(0.502f, 0.502f, 0.502f, 0.125f),

            CkCol.TriStateCheck     => new Vector4(0.000f, 0.816f, 0.000f, 1.000f),
            CkCol.TriStateCross     => new Vector4(0.816f, 0.000f, 0.000f, 1.000f),
            CkCol.TriStateNeutral   => new Vector4(0.816f, 0.816f, 0.816f, 1.000f),

            CkCol.IconOn            => 0xFF00D000.ToVec4(),
            CkCol.IconOff           => 0xFFD0D0D0.ToVec4(),

            _ => Vector4.Zero,
        };
}
