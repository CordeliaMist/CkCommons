using ImGuiNET;

namespace CkCommons.RichText;

public class SeperatorPayload : RichPayload
{
    public override int UpdateCache(ImFontPtr font, float wrapWidth, ref float curLineWidth)
    {
        // reset the current line width to 0 since we move to a new line.
        curLineWidth = 0f;
        return 1;
    }
}
