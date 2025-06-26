using CkCommons.Raii;
using ImGuiNET;

namespace CkCommons;

public class NewLinePayload : RichPayload
{
    public override void UpdateCache(ImFontPtr font, float wrapWidth, ref float curLineWidth)
    {
        curLineWidth = 0f;
    }
}
