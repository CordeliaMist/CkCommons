namespace CkCommons.RichText;

public interface IRichSegment
{
    // Shared Draw Method. (Maybe move in scale method idk)
    void Draw(RichStringContext ctx);

    // Shared Update Method
    void UpdateCache(ref RichStringContext ctx, int segmentIdx);
}