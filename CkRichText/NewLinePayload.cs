using ImGuiNET;

namespace CkCommons.RichText;

public class NewLinePayload : RichPayload
{
    public override int UpdateCache(ImFontPtr font, float wrapWidth, ref float curLineWidth)
    {
        curLineWidth = 0f;
        return 1;
    }
}
