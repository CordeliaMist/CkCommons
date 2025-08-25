using CkCommons.Helpers;
using CkCommons;
using CkCommons.Textures;
using FFXIVClientStructs.FFXIV.Common.Lua;
using Dalamud.Bindings.ImGui;
using Lumina.Excel.Sheets;
using System.Buffers.Binary;
using System.Diagnostics;
using Dalamud.Interface.Utility.Raii;

namespace CkCommons.RichText;

// If you ever care to support italics down the line use:
// https://github.com/Infiziert90/ChatTwo/blob/main/ChatTwo/FontManager.cs#L113
public class RichTextString
{
    /// <summary>
    ///     Represents a rich text string that can be colored, have images, or be bolded.
    ///     Payloads can add and take away from effects, and must be maintained in a style formatter.
    /// </summary>
    private readonly List<RichPayload> _payloads = new();

    private Stack<uint> _strokeColors = new();
    private bool _isValid;
    private ImFontPtr _lastFont;
    private float _lastWrapWidth;
    private int _lineCount;
    private uint? CurrentStroke => _strokeColors.Count > 0 ? _strokeColors.Peek() : null;
    private string RawText => string.Concat(_payloads.OfType<TextPayload>().Select(p => p.RawText));

    /// <summary>
    ///     An update cache will trigger after this due to the lastFont and width not being up to date.
    /// </summary>
    public RichTextString(string rawString)
    {
        BuildPayloads(rawString);
    }

    public int RichTextLineCount => _lineCount;

    /// <summary> Renders the combined richText for display. It is up to you to make sure the caches are valid. </summary>
    public void Render(ImFontPtr font, float wrapWidth)
    {
        using var _ = ImRaii.Group();
        // if there is a missmatch with the font pointer and wrapwidth, recalculate.
        if (!MatchesCachedState(font, wrapWidth))
        {
            if (CkRichText.DoLogging)
                Svc.Log.Information($"[RichText] Recalculating caches for font {font.GetDebugName()} and wrap width {wrapWidth}.");
            UpdateCaches(font, wrapWidth);
        }

        // If not valid, just display the textwrap unformatted.
        if (!_isValid)
        {
            ImGui.TextWrapped(RawText);
            return;
        }

        // Display the rich text.
        foreach (RichPayload payload in _payloads)
        {
            // do things based on the payload type.
            switch (payload)
            {
                // Invoke draw with the topmost pushed stroke. (text color is set already for us)
                case TextPayload text:
                    text.Draw(CurrentStroke);
                    break;

                // manipulate the ImGui.StyleColor stack for text color.
                case ColorPayload color:
                    color.UpdateColor();
                    break;

                // manipulate the current stroke color via RichTextStrings stroke color stack.
                case StrokePayload stroke:
                    stroke.UpdateStroke(ref _strokeColors);
                    break;

                // draws an image to ImGui.
                case ImagePayload image:
                    image.Draw();
                    break;

                case NewLinePayload:
                    ImGui.Spacing();
                    break;

                case SeperatorPayload:
                    ImGui.Separator();
                    break;
            }
        }
    }

    // must be manually invoked after construction.
    public unsafe void UpdateCaches(ImFontPtr font, float wrapWidth)
    {
        if (CkRichText.DoLogging)
            Svc.Log.Information($"[RichText] Recalculating caches for font {font.GetDebugName()} and wrap width {wrapWidth}.");
        // update the font and wrap width to the new value.
        _lastFont = font;
        _lastWrapWidth = wrapWidth;

        float currentLineWidth = 0f;
        float previousLineWidth = 0f;
        _lineCount = 1;
        // Update the individual caches to respect the new font and wrap width.
        foreach (RichPayload payload in _payloads)
        {
            _lineCount += payload.UpdateCache(font, wrapWidth, ref currentLineWidth);
            // update the previous width.
            previousLineWidth = currentLineWidth;
        }
    }

    public unsafe bool MatchesCachedState(ImFontPtr font, float wrapWidth)
        => _lastFont.Handle == font.Handle && _lastWrapWidth == wrapWidth;

    public void BuildPayloads(string rawText)
    {
        // update the payload to convert the \n and \r\n into paragraph and newline splits.
        rawText = rawText.Replace("\r\n", "\n"); // normalize newlines
        rawText = rawText.Replace("\n\n", "[para]");
        rawText = rawText.Replace("\n", "[para]");

        string[] result = CkRichText.RichTextRegex().Split(rawText); // [color, stroke]
        int[] valid = new int[2]; // [0] = color, [1] = stroke
        var sw = new Stopwatch();
        sw.Start();
        try
        {
            if (CkRichText.DoLogging)
                Svc.Log.Information($"[RichText] Parsing rich text string: {rawText}");
            
            foreach (string part in result)
            {
                if (string.IsNullOrWhiteSpace(part))
                    continue;

                if (CkRichText.DoLogging)
                    Svc.Log.Information($"[RichText] payload type was: {part}");

                // off switches.
                switch (part)
                {
                    case "[line]":
                        _payloads.Add(new SeperatorPayload());
                        continue;
                    case "[para]":
                        _payloads.Add(new NewLinePayload());
                        continue;
                    case "[/color]" or "[/rawcolor]":
                        _payloads.Add(ColorPayload.Off);
                        valid[0]--;
                        continue;
                    case "[/stroke]" or "[/glow]":
                        _payloads.Add(StrokePayload.Off);
                        valid[1]--;
                        continue;
                }

                // On Switches
                if (part.StartsWith("[rawcolor=", StringComparison.OrdinalIgnoreCase))
                {
                    // strip the [rawcolor= and ] from the part.
                    string colorValue = part[10..^1];

                    // parse out normal uint or hex uint.
                    uint color = 0;
                    bool success = colorValue.StartsWith("0x", StringComparison.OrdinalIgnoreCase)
                        ? uint.TryParse(colorValue[2..], System.Globalization.NumberStyles.HexNumber, null, out color)
                        : uint.TryParse(colorValue, out color);

                    if (!success)
                        throw new Exception($"[RichText] Invalid [rawcolor] tag value: {part}");
                    _payloads.Add(new ColorPayload(color));
                    valid[0]++;
                    continue;
                }

                if (part.StartsWith("[color=", StringComparison.OrdinalIgnoreCase))
                {
                    if (!TryParseColor(part[7..^1], out uint color))
                        throw new Exception($"[RichText] Invalid [color] tag value: {part}");

                    _payloads.Add(new ColorPayload(color));
                    valid[0]++;
                    continue;
                }

                if (part.StartsWith("[stroke=", StringComparison.OrdinalIgnoreCase))
                {
                    if (!TryParseColor(part[8..^1], out uint stroke))
                        throw new Exception($"[RichText] Invalid [stroke] tag value: {part}");

                    _payloads.Add(new StrokePayload(stroke));
                    valid[1]++;
                    continue;
                }

                if (part.StartsWith("[glow=", StringComparison.OrdinalIgnoreCase))
                {
                    if (!TryParseColor(part[6..^1], out uint stroke))
                        throw new Exception($"[RichText] Invalid [glow] tag value: {part}");

                    _payloads.Add(new StrokePayload(stroke));
                    valid[1]++;
                    continue;
                }

                // From Asset folder (dont let people be too exploitive lol)
                if (part.StartsWith("[img=", StringComparison.OrdinalIgnoreCase))
                {
                    string imgName = part[5..^1]; // strip [img= and ]
                    var assetImagePath = TextureManager.GetFullAssetPath(imgName);
                    if (!string.IsNullOrEmpty(assetImagePath))
                        _payloads.Add(new ImagePayload(assetImagePath));
                    else
                        _payloads.Add(new TextPayload(part)); // fallback to text if image not found.
                    continue;
                }

                if (part.StartsWith(":") && part.EndsWith(":") && part.Length > 2 && !part.Contains(" "))
                {
                    var emoteName = part[1..^1];
                    var emoteImagePath = TextureManager.GetFullEmotePath(emoteName);
                    if (!string.IsNullOrEmpty(emoteImagePath))
                        _payloads.Add(new ImagePayload(emoteImagePath));
                    else
                        _payloads.Add(new TextPayload(part));
                    continue;
                }

                // Otherwise just normal text payload.
                _payloads.Add(new TextPayload(part));
            }
            // all were valid.
            _isValid = true;
        }
        catch (Exception ex)
        {
            Svc.Log.Error($"Error while parsing rich text string: {rawText}\n{ex}");
            _payloads.Clear();
            _payloads.Add(new TextPayload(rawText));
            _isValid = false;
        }
        finally
        {
            sw.Stop();
            if (CkRichText.DoLogging)
                Svc.Log.Information($"[RichText] Parsed {_payloads.Count} payloads in {sw.ElapsedMilliseconds}ms. Colors: {valid[0]}, Strokes: {valid[1]}");
        }
    }

    private bool TryParseColor(string value, out uint color)
    {
        color = 0;
        // attempt to get the row id.
        if (ushort.TryParse(value, out ushort rowId))
        {
            // if it was vaid, get the UIColor row.
            if (Svc.Data.GetExcelSheet<UIColor>().GetRowOrDefault(rowId) is { } row && rowId != 0)
            {
                // the color will be the reverse endianness of the Dark value.
                color = BinaryPrimitives.ReverseEndianness(row.Dark);
                return true;
            }
        }
        // otherwise, it might be a named color, so try that.
        else if (Enum.TryParse<XlDataUiColor>(value, true, out XlDataUiColor namedColor))
        {
            // if valid, grab the rowId of that result.
            rowId = (ushort)namedColor;
            if (Svc.Data.GetExcelSheet<UIColor>().GetRowOrDefault(rowId) is { } row && rowId != 0)
            {
                color = BinaryPrimitives.ReverseEndianness(row.Dark);
                return true;
            }
        }
        return false;
    }
}
