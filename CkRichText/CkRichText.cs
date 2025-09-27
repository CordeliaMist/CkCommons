using CkCommons.Gui;
using CkCommons.Helpers;
using Dalamud.Interface.Colors;
using Dalamud.Bindings.ImGui;

namespace CkCommons.RichText;

/// <summary>
///     A class dedicated to mimicing the structure of dalamuds SeString composition but for ImGui. <para/>
///     CkRichText allows for composed strings of colors, normal text, outlined text, images, and emotes. <para/>
///     Supports proper text wrapping, with internal caching for optimal drawtime performance.
/// </summary>
public static partial class CkRichText
{
    private static ImFontPtr _currentFont  => ImGui.GetFont();
    private static float     _currentWidth => ImGui.GetContentRegionAvail().X;

    public static void DrawColorHelpText()
    {
        var tooltip = $"--COL--Named Color Codes:--COL----SEP--{string.Join(", ", Enum.GetNames<XlDataUiColor>())}";
        CkGui.HelpText(tooltip, ImGuiColors.ParsedPink);
    }

    // may be prone to flickering if done mid-text edit, look into more later.
    public static int GetRichTextLineHeight(string text, int cloneId)
    {
        if (!_cache.TryGetValue(new RichTextKey(cloneId, text), out var richString))
            return 0; // No size for empty text
        // Return the size of the rich text.
        return richString.RichTextLineCount;
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
    ///     [emote=Cappie] - EmoteTexture to display on the screen. <para/>
    ///     
    ///     For color number values, type the command "/xldata uicolor" into the in-game chat.
    /// </summary>
    public static void Text(ImFontPtr fontPtr, float wrapWidth, string text, int cloneId = 0)
    {
        var key = new RichTextKey(cloneId, text);
        _accessedKeys.Add(key); // Mark as accessed

        // If not cached, construct a new cache along with its internal payloads, and store it.
        if (!_cache.TryGetValue(key, out var richString))
        {
            richString = new RichTextString(text);
            _cache[key] = richString;
        }
        // Render the thingy.
        richString.Render(fontPtr, wrapWidth);
    }

    /// <summary>
    ///     Helper method to strip unwanted elements of a CkRichText rawstring, if desired.
    /// </summary>
    public static string StripDisallowedRichTags(string input, RichTextFilter allowed)
    {
        // return original if string is empty.
        if (string.IsNullOrWhiteSpace(input))
            return input;

        // account for excessive newline spam.
        input = input.Replace("\r\n", "\n").Replace("\n\n", "[para]").Replace("\n", "[para]");

        var result = new StringBuilder(input.Length);
        var tokens = RichTextRegex().Split(input);

        foreach (var t in tokens)
        {
            // If the token is empty, skip it.
            if (string.IsNullOrWhiteSpace(t))
                continue;

            // if the token has [], it is a tag.
            if (t.StartsWith("[") && t.EndsWith("]"))
            {
                var isAllowed = t switch
                {
                    "[line]" => (allowed & RichTextFilter.Line) != 0,
                    "[para]" => (allowed & RichTextFilter.Paragraph) != 0,
                    "[/color]" => (allowed & RichTextFilter.Color) != 0,
                    "[/rawcolor]" => (allowed & RichTextFilter.RawColor) != 0,
                    "[/stroke]" => (allowed & RichTextFilter.Stroke) != 0,
                    "[/glow]" => (allowed & RichTextFilter.Glow) != 0,
                    _ when t.StartsWith("[color=", StringComparison.OrdinalIgnoreCase) => (allowed & RichTextFilter.Color) != 0,
                    _ when t.StartsWith("[rawcolor=", StringComparison.OrdinalIgnoreCase) => (allowed & RichTextFilter.RawColor) != 0,
                    _ when t.StartsWith("[stroke=", StringComparison.OrdinalIgnoreCase) => (allowed & RichTextFilter.Stroke) != 0,
                    _ when t.StartsWith("[glow=", StringComparison.OrdinalIgnoreCase) => (allowed & RichTextFilter.Glow) != 0,
                    _ when t.StartsWith("[img=", StringComparison.OrdinalIgnoreCase) => (allowed & RichTextFilter.Images) != 0,
                    _ => true
                };
                if (isAllowed)
                    result.Append(t);
            }
            else
            {
                result.Append(t);
            }
        }
        return result.ToString();
    }


    [GeneratedRegex(@"(\[rawcolor=(?:0x[0-9a-fA-F]{1,8}|\d+)\])|(\[/rawcolor\])|(\[color=[0-9a-z#]+\])|(\[\/color\])|(\[stroke=[0-9a-z#]+\])|(\[\/stroke\])|(\[glow=[0-9a-z#]+\])|(\[\/glow\])|(\[img=[^\]]+\])|(:[^\s:]+:)|(\[para\])|(\[line\])", RegexOptions.IgnoreCase)]
    public static partial Regex RichTextRegex();

    // Compressed Version below, still untested.
    // [GeneratedRegex(@"(\[rawcolor=(?:0x[0-9a-fA-F]{1,8}|\d+)\])|(\[/(?:rawcolor|color|stroke|glow)\])|(\[(?:color|stroke|glow)=[0-9a-z#]+\])|(:[^\s:]+:)|(\[para\])|(\[line\])", RegexOptions.IgnoreCase)]
}
