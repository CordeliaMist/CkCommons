using Dalamud.Bindings.ImGui;
using OtterGui.Text;

namespace CkCommons.RichText;

public class EomjiSegment(string emoji) : IRichSegment
{
    // Not sure why we need this but can update overtime.
    /// <summary> if his image should be drawn inline or not. </summary>
    private bool _isInline = false;
    private Vector2 _size = new(ImGui.GetTextLineHeight());

    /// <summary> The name of the emoji to show. </summary>
    public string EmojiName => emoji;

    public bool IsSticker() => EmojiName.StartsWith("s~");

    public void Draw(RichStringContext ctx)
    {
        if (_isInline)
            ImGui.SameLine(0, 0);

        var showText = !NewRichText.ShowEmojis || NewRichText.EmojiLoader is null;

        if (!NewRichText.ShowEmojis)
        {
            ImGui.TextUnformatted($":{EmojiName}:");
            return;
        }
        if (NewRichText.EmojiLoader is null)
        {
            ImGui.TextUnformatted($":{EmojiName}:");
            return;
        }

        var lookupName = IsSticker() ? EmojiName[2..] : EmojiName;
        NewRichText.EmojiLoader.DrawEmoji(lookupName, _size);
    }

    public void UpdateCache(ref RichStringContext ctx, int segmentIdx)
    {
        if (!NewRichText.ShowEmojis || NewRichText.EmojiLoader is null)
        {
            var width = ImGui.CalcTextSize($":{EmojiName}:").X;
            if (ctx.CurrLineWidth + width > ctx.WrapWidth)
            {
                ctx.CurrLineWidth = width;
                ctx.LineCount++;
                _isInline = false;
            }
            else
            {
                ctx.CurrLineWidth += width;
                _isInline = segmentIdx > 1;
            }
            return;
        }

        var prevWidth = ctx.CurrLineWidth;
        // Assert the new currentLineWidth after the advance.
        int sizeScale = IsSticker() ? 4 : ctx.EmojisOnly ? 2 : 1;
        var height = sizeScale is 1
            ? ImGui.GetTextLineHeight()
            : (ImGui.GetTextLineHeight() * sizeScale) - ImUtf8.ItemSpacing.Y;
        _size = new Vector2(height);
        // Determine expected advancedX.
        var expectedX = ctx.CurrLineWidth + _size.X;
        if (expectedX > ctx.WrapWidth)
        {
            ctx.CurrLineWidth = 0f;
            _isInline = false;
            ctx.LineCount += sizeScale;
        }
        else
        {
            ctx.CurrLineWidth = expectedX;
            // Inline id dependant on if the previous curLineWIdth was 0 and us being on the first line.
            _isInline = !(ctx.LineCount is 1 && segmentIdx is 0);
        }
    }
}