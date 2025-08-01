using Dalamud.Interface;
using System.Numerics;

namespace CkCommons;

public enum CkColor
{
    // Colors used in remotes (from old version, subject to change)
    VibrantPink,
    VibrantPinkHovered,
    VibrantPinkPressed,

    CkMistressColor,
    CkMistressText,
    
    LushPinkLine,
    LushPinkLineDisabled,
    LushPinkButton,
    LushPinkButtonDisabled,

    RemoteBg,
    RemoteBgDark,
    RemoteLines,

    ButtonDrag,

    SideButton,
    SideButtonBG,

    // UI Element Components
    FancyHeader,
    FancyHeaderContrast,
    ElementHeader,
    ElementSplit,
    ElementBG,

    // Favoriting
    FavoriteStarOn,
    FavoriteStarHovered,
    FavoriteStarOff,

    // File System
    FolderExpanded,
    FolderCollapsed,
    FolderLine,

    // TriStateBoxes
    TriStateCheck,
    TriStateCross,
    TriStateNeutral,

    // IconCheckboxes
    IconCheckOn,
    IconCheckOff,
}

public static class CkColors
{
    public static Vector4 Vec4(this CkColor color)
        => color switch
        {
            CkColor.VibrantPink             => new Vector4(0.977f, 0.380f, 0.640f, 0.914f),
            CkColor.VibrantPinkHovered      => new Vector4(0.986f, 0.464f, 0.691f, 0.955f),
            CkColor.VibrantPinkPressed      => new Vector4(0.846f, 0.276f, 0.523f, 0.769f),

            CkColor.CkMistressColor         => new Vector4(0.886f, 0.407f, 0.658f, 1.000f),
            CkColor.CkMistressText          => new Vector4(1.000f, 0.711f, 0.843f, 1.000f),

            CkColor.LushPinkLine            => new Vector4(0.806f, 0.102f, 0.407f, 1.000f),
            CkColor.LushPinkLineDisabled    => new Vector4(0.806f, 0.102f, 0.407f, 0.500f),
            CkColor.LushPinkButton          => new Vector4(1.000f, 0.051f, 0.462f, 1.000f),
            CkColor.LushPinkButtonDisabled  => new Vector4(1.000f, 0.051f, 0.462f, 0.500f),

            CkColor.RemoteBg                => new Vector4(0.122f, 0.122f, 0.161f, 1.000f),
            CkColor.RemoteBgDark            => new Vector4(0.090f, 0.090f, 0.122f, 1.000f),
            CkColor.RemoteLines             => new Vector4(0.404f, 0.404f, 0.404f, 1.000f),

            CkColor.ButtonDrag              => new Vector4(0.097f, 0.097f, 0.097f, 0.930f),

            CkColor.SideButton              => new Vector4(0.451f, 0.451f, 0.451f, 1.000f),
            CkColor.SideButtonBG            => new Vector4(0.451f, 0.451f, 0.451f, 0.250f),

            // UI Editors.
            CkColor.FancyHeader             => new Vector4(0.579f, 0.170f, 0.359f, 0.828f),
            CkColor.FancyHeaderContrast     => new Vector4(0.100f, 0.022f, 0.022f, 0.299f),
            CkColor.ElementHeader           => new Vector4(1.000f, 0.181f, 0.715f, 0.825f),
            CkColor.ElementSplit            => new Vector4(0.180f, 0.180f, 0.180f, 1.000f),
            CkColor.ElementBG               => new Vector4(1.000f, 0.742f, 0.910f, 0.416f),
            
            CkColor.FolderExpanded          => new Vector4(0.753f, 0.941f, 1.000f, 1.000f),
            CkColor.FolderCollapsed         => new Vector4(0.753f, 0.941f, 1.000f, 1.000f),
            CkColor.FolderLine              => new Vector4(0.753f, 0.941f, 1.000f, 1.000f),
            
            CkColor.FavoriteStarOn          => new Vector4(0.816f, 0.816f, 0.251f, 1.000f),
            CkColor.FavoriteStarHovered     => new Vector4(0.816f, 0.251f, 0.816f, 1.000f),
            CkColor.FavoriteStarOff         => new Vector4(0.502f, 0.502f, 0.502f, 0.125f),
            
            CkColor.TriStateCheck           => new Vector4(0.000f, 0.816f, 0.000f, 1.000f),
            CkColor.TriStateCross           => new Vector4(0.816f, 0.000f, 0.000f, 1.000f),
            CkColor.TriStateNeutral         => new Vector4(0.816f, 0.816f, 0.816f, 1.000f),

            _ => Vector4.Zero,
        };

    public static uint Uint(this CkColor color)
        => color switch
        {
            CkColor.VibrantPink             => 0xE9A360F9,
            CkColor.VibrantPinkHovered      => 0xF3B076FB,
            CkColor.VibrantPinkPressed      => 0xC48546D7,

            CkColor.CkMistressColor         => CkColor.CkMistressColor.Vec4().ToUint(),
            CkColor.CkMistressText          => CkColor.CkMistressText.Vec4().ToUint(),

            CkColor.LushPinkLine            => 0xFF671ACD,
            CkColor.LushPinkButton          => 0xFF750DFF,

            CkColor.RemoteBg                => CkColor.RemoteBg.Vec4().ToUint(),
            CkColor.RemoteBgDark            => CkColor.RemoteBgDark.Vec4().ToUint(),
            CkColor.RemoteLines             => CkColor.RemoteLines.Vec4().ToUint(),

            CkColor.ButtonDrag              => 0xED181818,
            CkColor.SideButton              => 0xFF737373,
            CkColor.SideButtonBG            => 0x3F737373,

            // Main UI
            CkColor.FancyHeader             => CkColor.FancyHeader.Vec4().ToUint(),
            CkColor.FancyHeaderContrast     => CkColor.FancyHeaderContrast.Vec4().ToUint(),
            CkColor.ElementHeader           => 0xD2B62EFF,
            CkColor.ElementSplit            => 0xFF2D2D2D,
            CkColor.ElementBG               => 0x6AE8BDFF,

            CkColor.FolderExpanded          => 0xFFFFF0C0,
            CkColor.FolderCollapsed         => 0xFFFFF0C0,
            CkColor.FolderLine              => 0xFF40D0D0,

            CkColor.FavoriteStarOn          => CkColor.FavoriteStarOn.Vec4().ToUint(),
            CkColor.FavoriteStarHovered     => CkColor.FavoriteStarHovered.Vec4().ToUint(),
            CkColor.FavoriteStarOff         => CkColor.FavoriteStarOff.Vec4().ToUint(),

            CkColor.TriStateCheck           => 0xFFD040D0,
            CkColor.TriStateCross           => 0x20808080,
            CkColor.TriStateNeutral         => 0xFF00D000,

            CkColor.IconCheckOn             => 0xFF00D000,
            CkColor.IconCheckOff            => 0xFFD0D0D0,
            _                               => 0x00000000,
        };

    // Helper functions for when we add new colors
    public static uint ToUint(this Vector4 color) 
        => ColorHelpers.RgbaVector4ToUint(color);

    // Helper functions for when we add new colors
    public static Vector4 ToVector4(this uint color)
        => ColorHelpers.RgbaUintToVector4(color);

    // Down the line if we want we can add custom color themes from a palette here.
}
