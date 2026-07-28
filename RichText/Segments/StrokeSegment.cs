namespace CkCommons.RichText;

public class StrokeSegment(uint? color) : IRichSegment
{
    public uint? Color => color;

    public static StrokeSegment Off => new(null);

    public void Draw(RichStringContext ctx)
    {
        if (Color.HasValue)
            ctx.StrokeStack.Push(Color.Value);
        else
            ctx.StrokeStack.Pop();
    }

    public void UpdateCache(ref RichStringContext _, int __)
    { }
}