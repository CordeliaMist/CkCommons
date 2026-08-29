namespace CkCommons.RichText;

[Flags]
public enum RichTextFilter
{
    None = 0,
    Images = 1 << 0,
    Emotes = 1 << 1,
    Stickers = 1 << 2,
    Glow = 1 << 3,
    Stroke = 1 << 4,
    Color = 1 << 5,
    RawColor = 1 << 6,
    Paragraph = 1 << 7,
    Links = 1 << 8,
    Line = 1 << 9,

    All = Emotes | Images | Stickers | Glow | Stroke | Color | RawColor | Paragraph | Line | Links
}
