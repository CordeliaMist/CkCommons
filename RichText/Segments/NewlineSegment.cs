using Dalamud.Bindings.ImGui;

namespace CkCommons.RichText;

public class NewlineSegment : IRichSegment
{
    public void Draw(RichStringContext ctx)
        => ImGui.Spacing();

    public void UpdateCache(ref RichStringContext ctx, int __)
    {
        ctx.CurrLineWidth = 0f;
        ctx.LineCount++;
    }
}