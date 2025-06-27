using ImGuiNET;

namespace CkCommons.RichText;

public class StrokePayload : RichPayload
{
    public uint Color { get; }
    public StrokePayload(uint color)
        => Color = color;

    public static StrokePayload Off => new(0);

    public void UpdateStroke(ref Stack<uint> colorStrokes)
    {
        if (Color != 0)
            colorStrokes.Push(Color);
        else
            colorStrokes.Pop();
    }

    public override void UpdateCache(ImFontPtr _, float __, ref float ___)
    { }
}
