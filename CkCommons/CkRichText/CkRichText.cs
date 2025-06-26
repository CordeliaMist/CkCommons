using CkCommons.Gui;
using CkCommons.Helpers;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Textures.TextureWraps;
using ImGuiNET;

namespace CkCommons.RichText;

/// <summary>
///     A class dedicated to mimicing the structure of dalamuds SeString composition but for ImGui. <para/>
///     CkRichText allows for composed strings of colors, normal text, outlined text, images, and emotes. <para/>
///     Supports proper text wrapping, with internal caching for optimal drawtime performance.
/// </summary>
public static partial class CkRichText
{
    /// <summary> Represents a key for caching rich text strings. </summary>
    /// <remarks> The <paramref name="cloneId"/> helps stop rapid caching if in multiple windows. </remarks>
    private record RichTextKey(int cloneId, string rawText);

    // Cache for RichTextStrings to avoid re-creating them if already cached.
    private static readonly ConcurrentDictionary<RichTextKey, RichTextString> _cache = new();
    private static ImFontPtr _currentFont  => ImGui.GetFont();
    private static float     _currentWidth => ImGui.GetContentRegionAvail().X;

    // ------------------- Methods ------------------- //
    public static void DrawColorHelpText()
    {
        string tooltip = $"--COL--Named Color Codes:--COL----SEP--{string.Join(", ", Enum.GetNames<XlDataUiColor>())}";
        CkGui.HelpText(tooltip, ImGuiColors.ParsedPink);
    }

    /// <inheritdoc cref="Text(ImFontPtr, float, string, int)"/>/>
    public static void Text(string text, int cloneId = 0)
        => Text(_currentFont, _currentWidth, text, cloneId);

    /// <inheritdoc cref="Text(ImFontPtr, float, string, int)"/>/>
    public static void Text(float wrapWidth, string text, int cloneId = 0)
        => Text(_currentFont, wrapWidth, text, cloneId);

    /// <inheritdoc cref="Text(ImFontPtr, float, string, int)"/>/>
    public static void Text(ImFontPtr fontPtr, string text, int cloneId = 0)
        => Text(fontPtr, _currentWidth, text, cloneId);

    /// <summary>
    ///     Renders a rich text string, using the window as a cache key identifier.
    ///     If the same text is in multiple windows, use the method with an ID paramater. (Avoids rapid caching) <para/>
    /// 
    ///     HOW TO USE: <para/>
    ///     [color=red] color text by fancy name value. [/color] <para/>
    ///     [color=5] color text by xldata number value. [/color] <para/>
    ///     [stroke=red] turns the text into outlined text. [/stroke] <para/>
    ///     [stroke=5] turns the text into outlined text by xldata number value. [/stroke] <para/>
    ///     [img=path/to/image.png] - image from the Assets folder. <para/>
    ///     [emote=Cappie] - CoreEmoteTexture to display on the screen. <para/>
    ///     
    ///     For color number values, type the command "/xldata uicolor" into the in-game chat.
    /// </summary>
    public static void Text(ImFontPtr fontPtr, float wrapWidth, string text, int cloneId = 0)
    {
        RichTextKey key = new RichTextKey(cloneId, text);
        // If not cached, construct a new cache along with its internal payloads, and store it.
        if (!_cache.TryGetValue(key, out RichTextString? richString))
        {
            richString = new RichTextString(text);
            _cache[key] = richString;
        }
        // Render the thingy.
        richString.Render(fontPtr, wrapWidth);
    }
}
