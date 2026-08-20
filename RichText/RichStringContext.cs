using Dalamud.Bindings.ImGui;

namespace CkCommons.RichText;

/// <summary>
///   Contextual state struct for a RichString, used by its segments for recalculations and drawing.
/// </summary>
public struct RichStringContext(ImFontPtr font, float initX, float startX, float endX, bool emojiOnly)
{
    public ImFontPtr Font = font;
    public Stack<uint> StrokeStack = new();
    public bool EmojisOnly = emojiOnly;

    public float InitX = initX;
    public float LineStartX = startX;
    public float LineEndX = endX;
    public float CurrLineWidth = initX - startX;

    public int LineCount = 1;

    public readonly float WrapWidth => LineEndX - LineStartX;
    public readonly uint? CurrStroke => StrokeStack.Count > 0 ? StrokeStack.Peek() : null;

    public unsafe readonly bool MatchesContext(ImFontPtr otherFont, float initX, float startX, float endX)
        => Font.Handle == otherFont.Handle 
        && InitX == initX
        && LineStartX == startX 
        && LineEndX == endX;
}