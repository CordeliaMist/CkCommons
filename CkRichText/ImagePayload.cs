using Dalamud.Bindings.ImGui;

namespace CkCommons.RichText;

public class ImagePayload : RichPayload
{
    /// <summary> if his image should be drawn inline or not. </summary>
    private bool _isInline = false;

    /// <summary> The full filepath to load. This includes the assembly directory. </summary>
    public string _path { get; } = string.Empty;

    public ImagePayload(string fullImagePath) => _path = fullImagePath;

    /// <summary> Draws out the image to ImGui. </summary>
    public void Draw()
    {
        if (_isInline)
            ImGui.SameLine(0, 0);

        // draw based on texture validity.
        if (Svc.Texture.GetFromFile(_path).GetWrapOrDefault() is { } valid)
            ImGui.Image(valid.Handle, new Vector2(ImGui.GetTextLineHeight()));
        else
            ImGui.Dummy(new Vector2(ImGui.GetTextLineHeight())); // Fallback to dummy if texture is invalid.
    }

    public override int UpdateCache(ImFontPtr font, float wrapWidth, ref float curLineWidth, int lineCount)
    {
        var prevLineWidth = curLineWidth;

        // assert the new curLineWidth
        float newLineWidth = curLineWidth + ImGui.GetTextLineHeight();
        if (newLineWidth > wrapWidth)
        {
            curLineWidth = 0f;
            _isInline = false;
            return 1;
        }
        else
        {
            curLineWidth = newLineWidth;
            // Inline is dependant on if the previous curLineWidth was 0 and us being on the first line.
            _isInline = !(lineCount is 1 && prevLineWidth is 0f);
            return 0;
        }
    }
}
