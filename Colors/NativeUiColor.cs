using FFXIVClientStructs.FFXIV.Client.Graphics;
using System.Runtime.InteropServices;

namespace CkCommons;

[StructLayout(LayoutKind.Sequential)]
public readonly record struct NativeUiColor(uint Foreground = uint.MaxValue, uint Glow = uint.MinValue)
{
    public ByteColor TextByteColor() => new() { RGBA = Foreground | 0xFF000000 };
    public ByteColor EdgeByteColor() => new() { RGBA = Glow == uint.MinValue ? Glow : Glow | 0xFF000000 };
}