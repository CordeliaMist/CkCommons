using CkCommons.Gui;
using CkCommons.Helpers;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Lumina.Excel.Sheets;
using System.Buffers.Binary;
using System.Diagnostics;

namespace CkCommons.RichText;

// Can optimize further as we find need to, but otherwise should be ok.
public class NewRichString
{
    private readonly List<IRichSegment> _payloads = new();
    private bool _isValid;
    private RichStringContext _context;

    // UpdateCache triggers after due to _context.Font & width not matching.
    public NewRichString(string rawString)
        => BuildPayloads(rawString);

    /// <summary>
    ///   How much space the rendered text takes up.
    /// </summary>
    public Vector2 Size { get; private set; } = Vector2.Zero;

    /// <summary>
    ///   Do not use this for height calculations.
    /// </summary>
    public int LineCount => _context.LineCount;

    /// <summary>
    ///   If the text is invalid, this is printed instead.
    /// </summary>
    public string RawText => string.Concat(_payloads.OfType<TextSegment>().Select(p => p.RawText));

    /// <summary>
    ///   If the text is only Emojis, indicating they are larger than normal for display.
    /// </summary>
    public bool OnlyEmojis => _payloads.Count > 0 && _payloads.All(p => p is EomjiSegment || p is ColorSegment || p is StrokeSegment);

    public bool IsValid => _isValid;

    /// <summary>
    ///   Renders the text similar to ImGui.TextWrapped, with a customizable font display.
    /// </summary>
    public void RenderTextWrapped(ImFontPtr font, float wrapWidth)
    {
        // This should simulate ImGui.TextWrapped default behavior.
        var startX = ImGui.GetCursorPosX();
        var endX = startX + wrapWidth;

        // Check if dirty, based on what we know.
        var isDirty = !_context.MatchesContext(font, startX, startX, endX);
        // If dirty, ensure that we properly update the context and
        // segment calculations to the new paramaters.
        if (isDirty)
            UpdateCaches(font, startX, startX, endX);

        // Box everything drawn.
        using (ImRaii.Group())
        {
            // If not valid, display RawText in default ImGui.TextWrapped
            if (!_isValid)
            {
                ImGui.PushTextWrapPos(wrapWidth);
                ImGui.TextUnformatted(RawText);
                ImGui.PopTextWrapPos();
            }
            // Otherwise, draw out the segments and record the drawn size.
            else
            {
                foreach (var segment in _payloads)
                    segment.Draw(_context);
            }
        }
        // Store the size for DummyDrawing
        Size = ImGui.GetItemRectSize();
    }

    /// <summary>
    ///   Renders the TextWrapped similar to ImGui, using any desired font. <br/>
    ///   If the text is not visible, a Dummy will be drawn instead.
    /// </summary>
    public void RenderTextWrappedDummy(ImFontPtr font, float wrapWidth)
    {
        // This should simulate ImGui.TextWrapped default behavior.
        var startX = ImGui.GetCursorPosX();
        var endX = startX + wrapWidth;
        // Check if dirty, based on what we know.
        var isDirty = !_context.MatchesContext(font, startX, startX, endX);
        // If dirty, ensure that we properly update the context and segment calculations to the new paramaters.
        if (isDirty)
            UpdateCaches(font, startX, startX, endX);
        // If we have valid text, and the drawn area will not be visible, render the dummy.
        else if (_isValid && !CkGuiClip.IsNextItemVisible(Size))
        {
            ImGui.Dummy(Size);
            return;
        }

        // Otherwise, box the following so we can aquire its size, regardless of outcome.
        using (ImRaii.Group())
        {
            // If not valid, display RawText in default ImGui.TextWrapped
            if (!_isValid)
            {
                ImGui.PushTextWrapPos(wrapWidth);
                ImGui.TextUnformatted(RawText);
                ImGui.PopTextWrapPos();
            }
            // Otherwise, draw out the segments and record the drawn size.
            else
            {
                foreach (var segment in _payloads)
                    segment.Draw(_context);
            }
        }
        // Store the size for DummyDrawing
        Size = ImGui.GetItemRectSize();
    }

    public void RenderFlowTextWrapped(ImFontPtr font, float lineStartX, float? lineEndX)
    {
        var initX = ImGui.GetCursorPosX();
        var endX = lineEndX ?? initX + ImGui.GetContentRegionAvail().X;
        // Check if dirty, based on what we know.
        var isDirty = !_context.MatchesContext(font, initX, lineStartX, endX);
        // If dirty, ensure that we properly update the context and
        // segment calculations to the new paramaters.
        if (isDirty)
            UpdateCaches(font, initX, lineStartX, endX);

        // Box everything drawn.
        ImGui.SetCursorPosX(0);
        using (ImRaii.Group())
        {
            ImGui.SameLine(initX, 0);
            // If not valid, display RawText in default ImGui.TextWrapped
            if (!_isValid)
            {
                var splitX = ImGui.CalcWordWrapPositionA(font, ImGuiHelpers.GlobalScale, RawText, ImGui.GetContentRegionAvail().X);
                ImGui.TextUnformatted(RawText[..splitX]);
                // Ensure offset to the startX in the new line before pushing the text wrap.
                ImGui.NewLine();
                ImGui.SetCursorPosX(lineStartX);
                ImGui.PushTextWrapPos(endX - lineStartX);
                ImGui.TextUnformatted(RawText[splitX..]);
                ImGui.PopTextWrapPos();
            }
            // Otherwise, draw out the segments and record the drawn size.
            else
            {
                foreach (var segment in _payloads)
                    segment.Draw(_context);
            }
        }
        // Store the size for DummyDrawing
        Size = ImGui.GetItemRectSize();
    }

    public void RenderFlowTextWrappedDummy(ImFontPtr font, float lineStartX, float? lineEndX)
    {
        // This should simulate ImGui.TextWrapped default behavior.
        var initX = ImGui.GetCursorPosX();
        var endX = lineEndX ?? initX + ImGui.GetContentRegionAvail().X;
        // Check if dirty, based on what we know.
        var isDirty = !_context.MatchesContext(font, initX, lineStartX, endX);
        // If dirty, ensure that we properly update the context and
        // segment calculations to the new paramaters.
        if (isDirty)
            UpdateCaches(font, initX, lineStartX, endX);
        // If we have valid text, and the drawn area will not be visible, render the dummy.
        else if (_isValid && !CkGuiClip.IsNextItemVisible(Size))
        {
            ImGui.Dummy(Size);
            return;
        }

        // Otherwise, box the following so we can aquire its size, regardless of outcome.
        ImGui.SetCursorPosX(0);
        using (ImRaii.Group())
        {
            ImGui.SameLine(initX, 0);
            // If not valid, display RawText in default ImGui.TextWrapped
            if (!_isValid)
            {
                var splitX = ImGui.CalcWordWrapPositionA(font, ImGuiHelpers.GlobalScale, RawText, ImGui.GetContentRegionAvail().X);
                ImGui.TextUnformatted(RawText[..splitX]);
                // Ensure offset to the startX in the new line before pushing the text wrap.
                ImGui.NewLine();
                ImGui.SetCursorPosX(lineStartX);
                ImGui.PushTextWrapPos(endX - lineStartX);
                ImGui.TextUnformatted(RawText[splitX..]);
                ImGui.PopTextWrapPos();
            }
            // Otherwise, draw out the segments and record the drawn size.
            else
            {
                foreach (var segment in _payloads)
                    segment.Draw(_context);
            }
        }
        // Store the size for DummyDrawing
        Size = ImGui.GetItemRectSize();
    }

    public void UpdateCaches(ImFontPtr font, float curX, float startX, float endX)
    {
        if (CkRichText.DoLogging)
        {
            Svc.Log.Information($"[RichText] Recalculating caches @ startX={startX}, endX={endX}.");
            for (var i = 0; i < _payloads.Count; i++)
                Svc.Log.Information($"\tSegment type: {_payloads[i].GetType().Name}");
            Svc.Log.Information($"\tRawText: {RawText}");
        }
        // update the font and wrap width to the new value.
        _context = new RichStringContext(font, curX, startX, endX, OnlyEmojis);
        // Update the individual caches to respect the new font and wrap width.
        for (var i = 0; i < _payloads.Count; i++)
            _payloads[i].UpdateCache(ref _context, i);
    }

    // Remove this maybe!
    public bool MatchesCachedState(ImFontPtr font, float initX, float startX, float endX)
        => _context.MatchesContext(font, initX, startX, endX);

    public void BuildPayloads(string rawText)
    {
        // update the payload to convert the \n and \r\n into paragraph and newline splits.
        rawText = rawText.Replace("\r\n", "\n"); // normalize newlines
        //rawText = rawText.Replace("\n\n", "[para]"); // Double newline are large gaps
        rawText = rawText.Replace("\n", "[br]"); // Single newline are line breaks

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
                        _payloads.Add(new SeparatorSegment());
                        continue;
                    case "[br]":
                        _payloads.Add(new LineBreakSegment());
                        continue;
                    case "[para]":
                        _payloads.Add(new ParagraphSegment());
                        continue;
                    case "[/color]" or "[/rawcolor]":
                        _payloads.Add(ColorSegment.Off);
                        valid[0]--;
                        continue;
                    case "[/stroke]" or "[/glow]":
                        _payloads.Add(StrokeSegment.Off);
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
                    _payloads.Add(new ColorSegment(color));
                    valid[0]++;
                    continue;
                }

                if (part.StartsWith("[color=", StringComparison.OrdinalIgnoreCase))
                {
                    if (!TryParseColor(part[7..^1], out uint color))
                        throw new Exception($"[RichText] Invalid [color] tag value: {part}");

                    _payloads.Add(new ColorSegment(color));
                    valid[0]++;
                    continue;
                }

                if (part.StartsWith("[stroke=", StringComparison.OrdinalIgnoreCase))
                {
                    if (!TryParseColor(part[8..^1], out uint stroke))
                        throw new Exception($"[RichText] Invalid [stroke] tag value: {part}");

                    _payloads.Add(new StrokeSegment(stroke));
                    valid[1]++;
                    continue;
                }

                if (part.StartsWith("[glow=", StringComparison.OrdinalIgnoreCase))
                {
                    if (!TryParseColor(part[6..^1], out uint stroke))
                        throw new Exception($"[RichText] Invalid [glow] tag value: {part}");

                    _payloads.Add(new StrokeSegment(stroke));
                    valid[1]++;
                    continue;
                }

                // From Asset folder (dont let people be too exploitive lol)
                if (part.StartsWith("[img=", StringComparison.OrdinalIgnoreCase))
                {
                    string imgName = part[5..^1]; // strip [img= and ]
                    _payloads.Add(new ImageSegment(imgName));
                    continue;
                }

                if (part.StartsWith(":") && part.EndsWith(":") && part.Length > 2 && !part.Contains(" "))
                {
                    var emoteName = part[1..^1];
                    _payloads.Add(new EomjiSegment(emoteName));
                    continue;
                }

                // Otherwise just normal text payload.
                _payloads.Add(new TextSegment(part));
            }
            // all were valid.
            _isValid = true;
        }
        catch (Exception ex)
        {
            Svc.Log.Error($"Error while parsing rich text string: {rawText}\n{ex}");
            _payloads.Clear();
            _payloads.Add(new TextSegment(rawText));
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
        // Attempt first to see if it's a hexadecimal value, and if so, parse the direct hex from it
        if (value.StartsWith("0x", StringComparison.OrdinalIgnoreCase) && uint.TryParse(value[2..], System.Globalization.NumberStyles.HexNumber, null, out color))
            return true;
        // Otherwise, attempt to get the row id.
        else if (ushort.TryParse(value, out ushort rowId))
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
        else if (Enum.TryParse<XlDataUiColor>(value, true, out var namedColor))
        {
            // if valid, grab the rowId of that result.
            rowId = (ushort)namedColor;
            if (Svc.Data.GetExcelSheet<UIColor>().GetRowOrDefault(rowId) is { } row && rowId != 0)
            {
                color = BinaryPrimitives.ReverseEndianness(row.Dark);
                return true;
            }
        }

        // Raw AABBGGRR fallback
        return uint.TryParse(value, out color);
    }
}
