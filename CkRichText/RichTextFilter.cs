namespace CkCommons.RichText;

[Flags]
public enum RichTextFilter
{
    None = 0,
    Emotes = 1 << 0,
    Images = 1 << 1,
    Glow = 1 << 2,
    Stroke = 1 << 3,
    Color = 1 << 4,
    RawColor = 1 << 5,
    Paragraph = 1 << 6,
    Line = 1 << 7,
    All = Emotes | Images | Glow | Stroke | Color | RawColor | Paragraph | Line
}
