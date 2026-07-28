using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;

namespace CkCommons.RichText;

public class TextSegment(string text) : IRichSegment
{
    private (string LineText, float LineWidth)[] _splitCache = Array.Empty<(string, float)>();
    private bool _isInline = false;

    public string RawText => text;

    public void Draw(RichStringContext ctx)
    {
        if (_isInline)
            ImGui.SameLine(0, 0);

        // print text normally if there are no splits in the cached text.
        if (_splitCache.Length == 0)
        {
            if (ctx.CurrStroke.HasValue)
                TextOutlined(text, ctx.CurrStroke.Value);
            else
                ImGui.TextUnformatted(text);
        }
        else
        {
            if (ctx.CurrStroke.HasValue)
            {
                foreach (var (line, _) in _splitCache)
                    TextOutlined(line, ctx.CurrStroke.Value);
            }
            else
            {
                foreach (var (line, _) in _splitCache)
                    ImGui.TextUnformatted(line);
            }
        }
    }

    public void UpdateCache(ref RichStringContext ctx, int segmentIdx)
    {
        if (segmentIdx > 0 && ctx.CurrLineWidth != 0f)
            _isInline = true;

        var words = text.Split(' ');
        var lines = new List<(string line, float width)>();

        var charIndex = 0;
        var remainingWidth = ctx.WrapWidth - ctx.CurrLineWidth;
        var lineStart = 0;

        // for each word.
        for (var i = 0; i < words.Length; i++)
        {
            var word = words[i];
            // Get the word width.
            var wordWidth = 0f;
            foreach (var c in word)
                wordWidth += ctx.Font.GetCharAdvance(c);

            if (i > 0)
                wordWidth += ctx.Font.GetCharAdvance(' ');

            // Multiply by global scale to account for UI scaling.
            wordWidth *= ImGuiHelpers.GlobalScale;

            // If the word doesn't fit, we need to split the line.
            if (wordWidth > remainingWidth)
            {
                if (i == 0)
                {
                    // First word too wide to fit on current line, mark as not inline
                    _isInline = false;
                }
                else
                {
                    // Add current line, exclude trailing space
                    var length = charIndex - lineStart - 1;
                    if (length > 0)
                    {
                        var lineText = text.Substring(lineStart, length);
                        lines.Add((lineText, ctx.CurrLineWidth));
                    }
                }

                lineStart = charIndex;
                remainingWidth = ctx.WrapWidth;
                ctx.CurrLineWidth = 0; // reset to far left.
            }

            // Subtract the word's width from the remaining width (which reset to wrapwidth if split)
            remainingWidth -= wordWidth;
            ctx.CurrLineWidth += wordWidth;

            // add it as a split index.
            charIndex += word.Length;
            // add space char if not the last word
            if (i < words.Length - 1)
                charIndex += 1;
        }
        // Add the last line
        if (lineStart < text.Length)
        {
            var finalLineText = text.Substring(lineStart, charIndex - lineStart);
            lines.Add((finalLineText, ctx.CurrLineWidth));
        }

        _splitCache = lines.ToArray();
        ctx.CurrLineWidth = lines.Count > 0 ? lines[^1].width : 0f;
        ctx.LineCount += (lines.Count - 1);
    }

    private static void TextOutlined(string text, uint strokeColor)
    {
        var original = ImGui.GetCursorPos();
        using (ImRaii.PushColor(ImGuiCol.Text, strokeColor))
        {
            ImGui.SetCursorPos(original with { Y = original.Y-- });
            ImGui.TextUnformatted(text);
            ImGui.SetCursorPos(original with { X = original.X-- });
            ImGui.TextUnformatted(text);
            ImGui.SetCursorPos(original with { Y = original.Y++ });
            ImGui.TextUnformatted(text);
            ImGui.SetCursorPos(original with { X = original.X++ });
            ImGui.TextUnformatted(text);
            ImGui.SetCursorPos(original with { X = original.X--, Y = original.Y-- });
            ImGui.TextUnformatted(text);
            ImGui.SetCursorPos(original with { X = original.X++, Y = original.Y++ });
            ImGui.TextUnformatted(text);
            ImGui.SetCursorPos(original with { X = original.X--, Y = original.Y++ });
            ImGui.TextUnformatted(text);
            ImGui.SetCursorPos(original with { X = original.X++, Y = original.Y-- });
            ImGui.TextUnformatted(text);
        }

        ImGui.SetCursorPos(original);
        ImGui.TextUnformatted(text);
    }
}


