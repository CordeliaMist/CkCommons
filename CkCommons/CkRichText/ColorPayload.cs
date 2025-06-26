using ImGuiNET;

namespace CkCommons.RichText;

public static partial class CkRichText
{
    private class ColorPayload : RichPayload
    {
        public uint Color { get; }
        public ColorPayload(uint color)
        {
            Color = color;
        }

        public static ColorPayload Off => new(0);

        public void UpdateColor()
        {
            if (Color != 0)
                ImGui.PushStyleColor(ImGuiCol.Text, Color);
            else
                ImGui.PopStyleColor();
        }

        public override void UpdateCache(ImFontPtr _, float __, ref float ___)
        { }
    }
}
