using CkCommons.Gui;
using CkCommons.Helpers;
using Dalamud.Interface.Colors;
using Dalamud.Bindings.ImGui;

namespace CkCommons.RichText;

/// <summary>
///   A class dedicated to mimicing the structure of dalamuds SeString composition but for ImGui. <para/>
///   CkRichText allows for composed strings of colors, normal text, outlined text, images, and emotes. <para/>
///   Supports proper text wrapping, with internal caching for optimal drawtime performance.
/// </summary>
public static partial class NewRichText
{
    private static ImFontPtr _currentFont => ImGui.GetFont();
    private static float _currentWidth => ImGui.GetContentRegionAvail().X;

    public static void DrawColorHelpText()
    {
        var tooltip = $"--COL--Named Color Codes:--COL----SEP--{string.Join(", ", Enum.GetNames<XlDataUiColor>())}";
        CkGui.HelpTextFramed(tooltip, ImGuiColors.TankBlue);
    }

    /// <summary>
    ///   Quick way to retrieve how many lines total the wrapped text draws. <para/>
    ///   You should not use this to determine height, as emoji-only text can be larger.
    /// </summary>
    /// <remarks> This can return 0 if called on text you hide or don't draw. </remarks>
    public static int GetTextLineHeight(string text, string? id = null)
    {
        id ??= string.Empty;
        var key = new RichTextKey(id, text);
        if (_cache.TryGetValue(key, out var richString))
        {
            _accessedKeys.Add(key);
            return richString.LineCount;
        }
        return 0;
    }

    /// <summary>
    ///   Quick way to retrieve the rendered TextWrap or TextFlowWrap size.
    /// </summary>
    /// <remarks> This can return 0 if called on text you hide or don't draw. </remarks>
    public static Vector2 GetTextSize(string text, string? id = null)
    {
        id ??= string.Empty;
        var key = new RichTextKey(id, text);
        if (_cache.TryGetValue(key, out var richString))
        {
            _accessedKeys.Add(key);
            return richString.Size;
        }
        return Vector2.Zero;
    }

    #region TextWrap

    /// <inheritdoc cref="TextWrapped(ImFontPtr, string, float, string?)"/>/>
    public static void TextWrapped(string text, string? id = null)
        => TextWrapped(_currentFont, text, _currentWidth, id);

    /// <inheritdoc cref="TextWrapped(ImFontPtr, string, float, string?)"/>/>
    public static void TextWrapped(string text, float wrapWidth, string? id = null)
        => TextWrapped(_currentFont, text, wrapWidth, id);

    /// <inheritdoc cref="TextWrapped(ImFontPtr, string, float, string?)"/>/>
    public static void TextWrapped(ImFontPtr fontPtr, string text, string? id = null)
        => TextWrapped(fontPtr, text, _currentWidth, id);

    /// <summary>
    ///   Uses self-moderated caching to render pre-calculated texts, automatically adjusting
    ///   to resizing. If shown twice on screen, use ID param.
    ///   <para/>
    ///   HOW TO USE: <br/>
    ///   [color=red] color text by fancy name value. [/color] <br/>
    ///   [color=5] color text by xldata number value. [/color] <br/>
    ///   [stroke=red] turns the text into outlined text. [/stroke] <br/>
    ///   [stroke=5] turns the text into outlined text by xldata number value. [/stroke] <br/>
    ///   [img=path/to/image.png] - image from the Assets folder (Stems from defined root). <br/>
    ///   [emote=Cappie] - EmoteTexture to display on the screen. (Must define in EmojiLoader)
    ///   <para/>
    ///   For color number values, type the command "/xldata uicolor" into the in-game chat.
    /// </summary>
    public static void TextWrapped(ImFontPtr fontPtr, string text, float wrapWidth, string? id = null)
    {
        id ??= string.Empty;
        var key = new RichTextKey(id, text);
        _accessedKeys.Add(key); // Mark as accessed

        // If not cached, construct a new cache along with its internal payloads, and store it.
        if (!_cache.TryGetValue(key, out var richString))
        {
            richString = new NewRichString(text);
            _cache[key] = richString;
        }
        // Render the thingy.
        richString.RenderTextWrapped(fontPtr, wrapWidth);
    }

    /// <inheritdoc cref="TextWrappedOrDummy(ImFontPtr, string, float, string?)"/>
    public static void TextWrappedOrDummy(string text, string? id = null)
        => TextWrappedOrDummy(_currentFont, text, _currentWidth, id);

    /// <inheritdoc cref="TextWrappedOrDummy(ImFontPtr, string, float, string?)"/>
    public static void TextWrappedOrDummy(string text, float wrapWidth, string? id = null)
        => TextWrappedOrDummy(_currentFont, text, wrapWidth, id);

    /// <inheritdoc cref="TextWrappedOrDummy(ImFontPtr, string, float, string?)"/>
    public static void TextWrappedOrDummy(ImFontPtr fontPtr, string text, string? id = null)
        => TextWrappedOrDummy(fontPtr, text, _currentWidth, id);

    /// <summary>
    ///   Will render the textwrap, but if not visible, renders a dummy, helping performance. <para/>
    ///   <inheritdoc cref="TextWrapped(ImFontPtr, string, float, string?)"/>
    /// </summary>
    public static void TextWrappedOrDummy(ImFontPtr fontPtr, string text, float wrapWidth, string? id = null)
    {
        id ??= string.Empty;
        var key = new RichTextKey(id, text);
        _accessedKeys.Add(key); // Mark as accessed

        // If not cached, construct a new cache along with its internal payloads, and store it.
        if (!_cache.TryGetValue(key, out var richString))
        {
            richString = new NewRichString(text);
            _cache[key] = richString;
        }
        // Render the thingy.
        richString.RenderTextWrappedDummy(fontPtr, wrapWidth);
    }
    #endregion

    #region FlowWrap

    /// <inheritdoc cref="TextFlowWrapped(ImFontPtr, string, float, float?, string?)"/>
    public static void TextFlowWrapped(string text, float? lineEndX = null, string? id = null)
        => TextFlowWrapped(_currentFont, text, 0f, lineEndX, id);

    /// <inheritdoc cref="TextFlowWrapped(ImFontPtr, string, float, float?, string?)"/>
    public static void TextFlowWrapped(ImFontPtr fontPtr, string text, float? lineEndX = null, string? id = null)
        => TextFlowWrapped(fontPtr, text, 0f, lineEndX, id);

    /// <inheritdoc cref="TextFlowWrapped(ImFontPtr, string, float, float?, string?)"/>
    public static void TextFlowWrapped(string text, float lineStartX, float? lineEndX = null, string? id = null)
        => TextFlowWrapped(_currentFont, text, lineStartX, lineEndX, id);

    /// <summary>
    ///   A variant of TextWrapped that allows the text to start Will render the TextWrap. <br/>
    ///   If not visible, renders a dummy instead to help performance. <para/>
    ///   <inheritdoc cref="TextWrapped(ImFontPtr, float, string, string?)"/>
    /// </summary>
    public static void TextFlowWrapped(ImFontPtr fontPtr, string text, float lineStartX, float? lineEndX = null, string? id = null)
    {
        id ??= string.Empty;
        var key = new RichTextKey(id, text);
        _accessedKeys.Add(key); // Mark as accessed
        // If not cached, construct a new cache along with its internal payloads, and store it.
        if (!_cache.TryGetValue(key, out var richString))
        {
            richString = new NewRichString(text);
            _cache[key] = richString;
        }
        // Render the thingy.
        richString.RenderFlowTextWrapped(fontPtr, lineStartX, lineEndX);
    }

    public static void TextFlowWrappedOrDummy(string text, float? lineEndX = null, string? id = null)
        => TextFlowWrappedOrDummy(_currentFont, text, 0f, lineEndX, id);

    public static void TextFlowWrappedOrDummy(ImFontPtr fontPtr, string text, float? lineEndX = null, string? id = null)
        => TextFlowWrappedOrDummy(fontPtr, text, 0f, lineEndX, id);

    public static void TextFlowWrappedOrDummy(string text, float lineStartX, float? lineEndX = null, string? id = null)
        => TextFlowWrappedOrDummy(_currentFont, text, lineStartX, lineEndX, id);

    public static void TextFlowWrappedOrDummy(ImFontPtr fontPtr, string text, float lineStartX, float? lineEndX = null, string? id = null)
    {
        id ??= string.Empty;
        var key = new RichTextKey(id, text);
        _accessedKeys.Add(key); // Mark as accessed
        // If not cached, construct a new cache along with its internal payloads, and store it.
        if (!_cache.TryGetValue(key, out var richString))
        {
            richString = new NewRichString(text);
            _cache[key] = richString;
        }
        // Render the thingy.
        richString.RenderFlowTextWrappedDummy(fontPtr, lineStartX, lineEndX);
    }

    #endregion

    /// <summary>
    ///   Helper method to strip unwanted elements of a CkRichText rawstring, if desired.
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
            // Handle Emotes/Stickers
            else if (t.Length > 2 && t[0] == ':' && t[^1] == ':')
            {
                // Stickers begin with s~
                if (t.StartsWith(":s~", StringComparison.OrdinalIgnoreCase))
                {
                    if ((allowed & RichTextFilter.Stickers) != 0)
                        result.Append(t);
                    // If emotes are allowed but this is a sticker, append it in emoji format.
                    else if ((allowed & RichTextFilter.Emotes) != 0)
                        result.Append(':').Append(t.AsSpan(3));
                }
                // Otherwise filter out normal emojis if not allowed.
                else
                {
                    if ((allowed & RichTextFilter.Emotes) != 0)
                        result.Append(t);
                }
            }
            else
            {
                result.Append(t);
            }
        }
        return result.ToString();
    }


    [GeneratedRegex(@"(\[rawcolor=(?:0x[0-9a-fA-F]{1,8}|\d+)\])|(\[/rawcolor\])|(\[color=[0-9a-z#]+\])|(\[\/color\])|(\[stroke=[0-9a-z#]+\])|(\[i\])|(\[\/i\])|(\[\/stroke\])|(\[glow=[0-9a-z#]+\])|(\[\/glow\])|(\[img=[^\]]+\])|(:[^:\[\]\s]+:)|(\[para\])|(\[line\])", RegexOptions.IgnoreCase)]
    public static partial Regex RichTextRegex();

    // Compressed Version below, still untested.
    // [GeneratedRegex(@"(\[rawcolor=(?:0x[0-9a-fA-F]{1,8}|\d+)\])|(\[/(?:rawcolor|color|stroke|glow)\])|(\[(?:color|stroke|glow)=[0-9a-z#]+\])|(:[^\s:]+:)|(\[para\])|(\[line\])", RegexOptions.IgnoreCase)]
}
