using CkCommons;
using Dalamud.Interface.Textures.TextureWraps;
using Dalamud.Bindings.ImGui;
using System.IO;

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

    public override int UpdateCache(ImFontPtr font, float wrapWidth, ref float curLineWidth)
    {
        if (curLineWidth != 0f)
            _isInline = true;

        // assert the new curLineWidth
        float newLineWidth = curLineWidth + ImGui.GetTextLineHeight();
        if (newLineWidth > wrapWidth)
        {
            curLineWidth = 0f;
            return 1;
        }
        else
        {
            curLineWidth = newLineWidth;
            return 0;
        }
    }
}
