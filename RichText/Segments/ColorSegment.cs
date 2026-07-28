using Dalamud.Bindings.ImGui;

namespace CkCommons.RichText;

public class ColorSegment(uint? color) : IRichSegment
{
    public uint? Color => color;

    public static ColorSegment Off => new(null);

    public void Draw(RichStringContext _)
    {
        if (Color.HasValue)
            ImGui.PushStyleColor(ImGuiCol.Text, Color.Value);
        else
            ImGui.PopStyleColor();
    }

    public void UpdateCache(ref RichStringContext _, int __)
    { }
}