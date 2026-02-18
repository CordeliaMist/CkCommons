using Dalamud.Bindings.ImGui;

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

    public override int UpdateCache(ImFontPtr _, float __, ref float ___, int ____)
        => 0;
}
