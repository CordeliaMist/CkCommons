using Dalamud.Bindings.ImGui;

namespace CkCommons.RichText;

public class LineBreakSegment : IRichSegment
{
    private bool _forceEmptyLine = false;

    public void Draw(RichStringContext ctx)
    {
        if (_forceEmptyLine) ImGui.NewLine();
    }

    public void UpdateCache(ref RichStringContext ctx, int __)
    {
        _forceEmptyLine = ctx.CurrLineWidth == 0f || ctx.CurrLineWidth == ctx.LineStartX;
        // Update the result.
        ctx.CurrLineWidth = 0f;
        ctx.LineCount++;
    }
}

public class ParagraphSegment : IRichSegment
{
    public void Draw(RichStringContext ctx)
    {
        ImGui.NewLine();
    }

    public void UpdateCache(ref RichStringContext ctx, int __)
    {
        ctx.CurrLineWidth = 0f;
        ctx.LineCount += 2;
    }
}