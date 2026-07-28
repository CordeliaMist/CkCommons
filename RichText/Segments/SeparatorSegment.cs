using Dalamud.Bindings.ImGui;

namespace CkCommons.RichText;

public class SeparatorSegment : IRichSegment
{
    public void Draw(RichStringContext ctx)
        => ImGui.Separator();

    public void UpdateCache(ref RichStringContext ctx, int __)
    {
        ctx.CurrLineWidth = 0f;
        ctx.LineCount++;
    }
}