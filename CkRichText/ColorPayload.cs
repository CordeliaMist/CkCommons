using ImGuiNET;

namespace CkCommons.RichText;

public class ColorPayload : RichPayload
{
    public uint? Color { get; }
    public ColorPayload(uint? color)
    {
        Color = color;
    }

    public static ColorPayload Off => new(null);

    public void UpdateColor()
    {
        if (Color.HasValue)
            ImGui.PushStyleColor(ImGuiCol.Text, Color.Value);
        else
            ImGui.PopStyleColor();
    }

    public override int UpdateCache(ImFontPtr _, float __, ref float ___)
        => 0;
}
