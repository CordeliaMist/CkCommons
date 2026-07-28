using CkCommons.Gui;
using Dalamud.Bindings.ImGui;
using System.IO;

namespace CkCommons.RichText;

public class ImageSegment : IRichSegment
{
    private bool _isInline = false;

    public ImageSegment(string imagePath)
    {
        ImagePath = imagePath;
        ImageSize = new(ImGui.GetTextLineHeight());
    }

    public ImageSegment(string imagePath, Vector2 size)
    {
        ImagePath = imagePath;
        ImageSize = size;
    }

    public string ImagePath { get; init; }
    public Vector2 ImageSize { get; init; }

    public void Draw(RichStringContext ctx)
    {
        if (_isInline)
            ImGui.SameLine(0, 0);

        if (NewRichText.ImageRootPath is { } rootPath)
        {
            var path = Path.Combine(rootPath, ImagePath);
            if (Svc.Texture.GetFromFile(path).GetWrapOrDefault() is { } img)
                ImGui.Image(img.Handle, ImageSize);
            else
                ImGui.Dummy(ImageSize);
        }
        else
            ImGui.Dummy(ImageSize);
        // Tooltip it.
        CkGui.AttachTooltip($"(Path: {ImagePath})");
    }

    public void UpdateCache(ref RichStringContext ctx, int __)
    {
        var prevLineWidth = ctx.CurrLineWidth;
        // assert the new curLineWidth
        float newLineWidth = ctx.CurrLineWidth + ImageSize.X;
        if (newLineWidth > ctx.WrapWidth)
        {
            ctx.CurrLineWidth = ctx.LineStartX;
            _isInline = false;
            ctx.LineCount++;
        }
        else
        {
            ctx.CurrLineWidth = ctx.LineStartX + newLineWidth;
            // Inline is dependant on if the previous curLineWidth was 0 and us being on the first line.
            _isInline = !(ctx.LineCount is 1 && prevLineWidth is 0f);
        }
    }
}