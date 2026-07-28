using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;

namespace CkCommons;

// Pulled from old Mare Code to be used temporarily while things get figured out.
// There are more efficient ways to do this, and we can explore that later, but not now.
public static class SeStringBuilderExtensions
{
    private const byte _colorTypeForeground = 0x13;
    private const byte _colorTypeGlow = 0x14;

    private static RawPayload BuildColorStartPayload(byte colorType, uint color)
        => new(unchecked([0x02, colorType, 0x05, 0xF6, byte.Max((byte)color, 0x01), byte.Max((byte)(color >> 8), 0x01), byte.Max((byte)(color >> 16), 0x01), 0x03]));

    private static RawPayload BuildColorEndPayload(byte colorType)
        => new([0x02, colorType, 0x02, 0xEC, 0x03]);

    public static void AddColoredText(this SeStringBuilder builder, string text, NativeUiColor colors)
    {
        if (colors.Foreground != default)
            builder.BeginForegroundColor(colors.Foreground);
        if (colors.Glow != default)
            builder.BeginGlowColor(colors.Glow);
        builder.AddText(text);
        if (colors.Glow != default)
            builder.EndGlowColor();
        if (colors.Foreground != default)
            builder.EndForegroundColor();
    }

    public static void BeginForegroundColor(this SeStringBuilder sb, uint color)
        => sb.Add(BuildColorStartPayload(_colorTypeForeground, color));

    public static void EndForegroundColor(this SeStringBuilder sb)
        => sb.Add(BuildColorEndPayload(_colorTypeForeground));

    public static void BeginGlowColor(this SeStringBuilder sb, uint color)
        => sb.Add(BuildColorStartPayload(_colorTypeGlow, color));

    public static void EndGlowColor(this SeStringBuilder sb)
        => sb.Add(BuildColorEndPayload(_colorTypeGlow));
}