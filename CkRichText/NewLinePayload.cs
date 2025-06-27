using ImGuiNET;

namespace CkCommons.RichText;

public static partial class CkRichText
{
    private class NewLinePayload : RichPayload
    {
        public override void UpdateCache(ImFontPtr font, float wrapWidth, ref float curLineWidth)
        {
            curLineWidth = 0f;
        }
    }
}
