using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using static FFXIVClientStructs.STD.Helper.IStaticMemorySpace;

namespace CkCommons;

/// <summary>
///  Colors used for CkCommons Internal style elements.
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
    ///  The total number of ColorVars in CkCommons
    /// </summary>
    Count,
}

public struct ColorMod
{
    public CkCol Var;
    public Vector4 BackupVec4;
    public uint BackupU32;
}

public static class CkColors
{
    public static readonly int Count = Enum.GetValues<CkCol>().Length;

    // Stores both Vec4 and uint for immidiate access.
    private static readonly Vector4[] _vec4 = new Vector4[Count];
    private static readonly uint[] _u32 = new uint[Count];

    // Stores any pushed/popped color mods of the stack.
    private static readonly ColorMod[] _stack = new ColorMod[256];
    private static int _stackTop;

    // Runs once, ensures all _vec4 and _u32 are populated immediately
    static CkColors()
    {
        foreach (var kvp in Defaults)
        {
            int index = (int)kvp.Key;
            _vec4[index] = kvp.Value;
            _u32[index] = kvp.Value.ToUint();
        }
    }

    #region Casts
    public static Dictionary<CkCol, Vector4> AsVec4Dictionary()
        => Enumerable.Range(0, Count).ToDictionary(i => (CkCol)i, i => _vec4[i]);

    public static Dictionary<CkCol, uint> AsUintDictionary()
        => Enumerable.Range(0, Count).ToDictionary(i => (CkCol)i, i => _u32[i]);
    #endregion

    public static int StackSize => _stackTop;

    /// <summary>
    ///   Gets the <see cref="uint"/> value of the <see cref="CkCol"/>
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint Uint(this CkCol col)
        => _u32[(int)col];

    /// <summary>
    ///   Gets the <see cref="Vector4"/> value of the <see cref="CkCol"/>
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector4 Vec4(this CkCol col)
        => _vec4[(int)col];

    #region Updaters
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Set(CkCol var, Vector4 col)
    {
        Debug.Assert(_stackTop == 0, "Do not modify base colors while a stack is active!");
        _vec4[(int)var] = col;
        _u32[(int)var] = col.ToUint();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Set(CkCol var, uint col)
    {
        Debug.Assert(_stackTop == 0, "Do not modify base colors while a stack is active!");
        _u32[(int)var] = col;
        _vec4[(int)var] = col.ToVec4();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void RevertCol(CkCol col)
    {
        Debug.Assert(_stackTop == 0, "Do not revert base colors while a stack is active!");
        var defaultCol = Defaults[col];
        _vec4[(int)col] = defaultCol;
        _u32[(int)col] = defaultCol.ToUint();
    }

    public static void RevertAll()
    {
        Debug.Assert(_stackTop == 0, "Do not revert base colors while a stack is active!");
        foreach (var kvp in Defaults)
        {
            int index = (int)kvp.Key;
            _vec4[index] = kvp.Value;
            _u32[index] = kvp.Value.ToUint();
        }
    }
    #endregion

    #region Base Push / Pop (Low Level)
    // Maybe apply AggressiveOptimization to these if we get better performance with it.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void PushColor(CkCol var, Vector4 color)
    {
        Debug.Assert(_stackTop < _stack.Length, "Stack overflow in PushColor");
        int index = (int)var;
        _stack[_stackTop++] = new ColorMod { Var = var, BackupVec4 = _vec4[index], BackupU32 = _u32[index] };
        _vec4[index] = color;
        _u32[index] = color.ToUint();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void PushColor(CkCol var, uint color)
    {
        Debug.Assert(_stackTop < _stack.Length, "Stack overflow in PushColor");
        int index = (int)var;
        _stack[_stackTop++] = new ColorMod { Var = var, BackupVec4 = _vec4[index], BackupU32 = _u32[index] };
        _u32[index] = color;
        _vec4[index] = color.ToVec4();
    }

    public static void PopColor(int count = 1)
    {
        Debug.Assert(_stackTop >= count, "Stack underflow in PopColor");
        while (count-- > 0)
        {
            var mod = _stack[--_stackTop];
            int index = (int)mod.Var;
            _vec4[index] = mod.BackupVec4;
            _u32[index] = mod.BackupU32;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void PopColor()
        => PopColor(1);
    #endregion

    #region Disposable Usings
    /// <summary> 
    ///   Starts a chainable, disposable color push.
    /// </summary>
    public static ColorDisposable Push(CkCol var, Vector4 color, bool condition = true)
        => new ColorDisposable().Push(var, color, condition);

    /// <summary>
    ///   Starts a chainable, disposable color push.
    /// </summary>
    public static ColorDisposable Push(CkCol var, uint color, bool condition = true)
        => new ColorDisposable().Push(var, color, condition);

    /// <summary>
    ///   Automatically tracks and pops pushed colors when disposed.
    /// </summary>
    public struct ColorDisposable : IDisposable
    {
        public int PushedCount { get; private set; }

        public ColorDisposable Push(CkCol var, Vector4 color, bool condition = true)
        {
            if (condition)
            {
                PushColor(var, color);
                PushedCount++;
            }
            return this; // Returns itself to allow chaining
        }

        public ColorDisposable Push(CkCol var, uint color, bool condition = true)
        {
            if (condition)
            {
                PushColor(var, color);
                PushedCount++;
            }
            return this;
        }

        public void Dispose()
        {
            if (PushedCount > 0)
            {
                PopColor(PushedCount);
                PushedCount = 0;
            }
        }
    }
    #endregion



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
    ///   The Default CkColors. <br />
    ///   List is extensive, and can likely optimize to trim down.
    /// </summary>
    public static readonly IReadOnlyDictionary<CkCol, Vector4> Defaults = new Dictionary<CkCol, Vector4>
    {
        // General Colors
        { CkCol.Help,              new Vector4(0.500f, 0.500f, 0.500f, 1.000f) },
        { CkCol.HelpHovered,       new Vector4(0.000f, 0.600f, 1.000f, 1.000f) },
        { CkCol.HelpActive,        new Vector4(0.000f, 0.800f, 1.000f, 1.000f) },
        { CkCol.HelpDisabled,      new Vector4(0.350f, 0.350f, 0.350f, 1.000f) },

        { CkCol.ProgressBar,       new Vector4(0.977f, 0.380f, 0.640f, 0.914f) }, // Placeholder

        { CkCol.BoxItem,           new Vector4(0.000f, 0.000f, 0.000f, 0.000f) }, // ADD_LATER
        { CkCol.BoxItemHovered,    new Vector4(0.000f, 0.000f, 0.000f, 0.000f) }, // ADD_LATER
        { CkCol.BoxItemActive,     new Vector4(0.000f, 0.000f, 0.000f, 0.000f) }, // ADD_LATER
        { CkCol.BoxItemDisabled,   new Vector4(0.000f, 0.000f, 0.000f, 0.000f) }, // ADD_LATER
        { CkCol.BoxItemFrame,      new Vector4(0.000f, 0.000f, 0.000f, 0.000f) }, // ADD_LATER

        // Tooltips
        { CkCol.TipFrame,          new Vector4(0.977f, 0.380f, 0.640f, 0.914f) },

        // CkGui.ColorSeparator
        { CkCol.Divider,           new Vector4(0.145f, 0.157f, 0.204f, 1.000f) },
        { CkCol.DividerHovered,    new Vector4(0.180f, 0.195f, 0.255f, 1.000f) },
        { CkCol.DividerActive,     new Vector4(0.220f, 0.240f, 0.320f, 1.000f) },

        // Favorite Star Utils
        { CkCol.Favorite,          new Vector4(0.816f, 0.816f, 0.251f, 1.000f) },
        { CkCol.FavoriteHovered,   new Vector4(0.816f, 0.251f, 0.816f, 1.000f) },
        { CkCol.FavoriteOff,       new Vector4(0.502f, 0.502f, 0.502f, 0.125f) },

        // TriStateBoxes
        { CkCol.TriStateCheck,     new Vector4(0.000f, 0.816f, 0.000f, 1.000f) },
        { CkCol.TriStateCross,     new Vector4(0.816f, 0.000f, 0.000f, 1.000f) },
        { CkCol.TriStateNeutral,   new Vector4(0.816f, 0.816f, 0.816f, 1.000f) },

        // Other Boxes
        { CkCol.IconOn,            new Vector4(0.000f, 0.816f, 0.000f, 1.000f) },
        { CkCol.IconOff,           new Vector4(0.816f, 0.816f, 0.816f, 1.000f) },

        // File System
        { CkCol.FsFolderOpen,      new Vector4(1.000f, 1.000f, 1.000f, 1.000f) },
        { CkCol.FsFolderClose,     new Vector4(1.000f, 1.000f, 1.000f, 1.000f) },
        { CkCol.FsFolderLine,      new Vector4(1.000f, 1.000f, 1.000f, 1.000f) },

        // DrawSystem
        { CkCol.DdsGroupIcon,      new Vector4(1.000f, 1.000f, 1.000f, 1.000f) },
        { CkCol.DdsGroupBg,        new Vector4(1.000f, 1.000f, 1.000f, 1.000f) },
        { CkCol.DdsGroupBorder,    new Vector4(1.000f, 1.000f, 1.000f, 1.000f) },
        { CkCol.DdsGroupGradient,  new Vector4(0.500f, 0.500f, 0.500f, 1.000f) }, // Multiplied by fade intensity?
        
        // Maybe merge these two groups into one set of "DrawSystemFolder" colors?
        { CkCol.DdsFolderIcon,     new Vector4(1.000f, 1.000f, 1.000f, 1.000f) },
        { CkCol.DdsFolderBg,       new Vector4(1.000f, 1.000f, 1.000f, 1.000f) },
        { CkCol.DdsFolderBorder,   new Vector4(1.000f, 1.000f, 1.000f, 1.000f) },
        { CkCol.DdsFolderGradient, new Vector4(0.500f, 0.500f, 0.500f, 1.000f) }, // Multiplied by fade intensity?

        { CkCol.DdsNodeLine,       new Vector4(1.000f, 1.000f, 1.000f, 1.000f) },
        { CkCol.DdsLeafBg,         new Vector4(0.050f, 0.050f, 0.050f, 1.000f) },

        // CkRaii.Child & Variants
        { CkCol.ChildFrame,        new Vector4(0.806f, 0.102f, 0.407f, 1.000f) }, // Placeholder

        // CkRaii.HeaderChild
        { CkCol.HChild,            new Vector4(1.000f, 0.181f, 0.715f, 0.825f) },
        { CkCol.HChildHovered,     new Vector4(0.000f, 0.000f, 0.000f, 0.000f) }, // ADD_LATER
        { CkCol.HChildActive,      new Vector4(0.000f, 0.000f, 0.000f, 0.000f) }, // ADD_LATER
        { CkCol.HChildDisabled,    new Vector4(0.000f, 0.000f, 0.000f, 0.000f) }, // ADD_LATER
        { CkCol.HChildSplit,       new Vector4(0.180f, 0.180f, 0.180f, 1.000f) },
        { CkCol.HChildBg,          new Vector4(1.000f, 0.742f, 0.910f, 0.416f) },
        { CkCol.HChildFrame,       new Vector4(0.000f, 0.000f, 0.000f, 0.000f) }, // ADD_LATER_MAYBE

        // CkRaii.LabeledChilds
        { CkCol.LChild,            new Vector4(0.977f, 0.380f, 0.640f, 0.914f) }, // Placeholder (GsCol Pink)
        { CkCol.LChildHovered,     new Vector4(0.986f, 0.464f, 0.691f, 0.955f) }, // Placeholder (GsCol PinkHovered)
        { CkCol.LChildActive,      new Vector4(0.846f, 0.276f, 0.523f, 0.769f) }, // Placeholder (GsCol PinkActive)
        { CkCol.LChildDisabled,    new Vector4(0.000f, 0.000f, 0.000f, 0.000f) }, // ADD_LATER
        { CkCol.LChildSplit,       new Vector4(0.180f, 0.180f, 0.180f, 1.000f) }, // Placeholder (HChildSplit)
        { CkCol.LChildBg,          new Vector4(0.579f, 0.170f, 0.359f, 0.828f) }, // Placeholder (GsCol PinkHovered)
        { CkCol.LChildFrame,       new Vector4(0.000f, 0.000f, 0.000f, 0.000f) }, // ADD_LATER

        // CkRaii.CurvedHeader
        { CkCol.CurvedHeader,      new Vector4(0.579f, 0.170f, 0.359f, 0.828f) },
        { CkCol.CurvedHeaderFade,  new Vector4(0.100f, 0.022f, 0.022f, 0.299f) },

        // CKRaii.FancySearch
        { CkCol.SearchBg,          new Vector4(0.290f, 0.290f, 0.290f, 0.540f) },
        { CkCol.SearchFrame,       new Vector4(0.180f, 0.180f, 0.180f, 1.000f) }, // TUNE_LATER

        // CkRaii.FancyTabBar
        { CkCol.TabBar,            new Vector4(0.579f, 0.170f, 0.359f, 0.828f) }, // Placeholder (CurvedHeader)
        { CkCol.TabHovered,        new Vector4(0.986f, 0.464f, 0.691f, 0.955f) }, // Placeholder (GsCol PinkHovered)
        { CkCol.TabActive,         new Vector4(0.977f, 0.380f, 0.640f, 0.914f) }, // Placeholder (GsCol Pink)
        { CkCol.TabDisabled,       new Vector4(0.000f, 0.000f, 0.000f, 0.000f) }, // DARK_PINK_ADD_LATER
        { CkCol.TabBarSplit,       new Vector4(0.000f, 0.000f, 0.000f, 0.000f) }, // ADD_LATER
        { CkCol.TabBarFrame,       new Vector4(0.579f, 0.170f, 0.359f, 0.828f) }, // Placeholder (CurvedHeader)
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

    /// <summary>
    ///   Inverts the RGB values of a uint color, excluding opacity. 
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint InvertColor(this uint x) => x ^ 0x00FFFFFFu;

    /// <summary>
    ///   Inverts the RGB values of a uint color and sets its opacity percent. (0.0f to 1.0f). 
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint InvertColor(this uint x, float opacity)
    {
        opacity = Math.Clamp(opacity, 0f, 1f);
        var  newAlpha = (uint)(opacity * 255f) << 24;
        // Invert the RGB, clear original alpha, then apply new alpha.
        return ((x ^ 0x00FFFFFFu) & 0x00FFFFFFu) | newAlpha;
    }

    /// <summary>
    ///   Inverts the RGB values of a uint color, including opacity. 
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint InvertColorFull(this uint x) => x ^ 0xFFFFFFFFu;
}
