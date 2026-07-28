using Dalamud.Interface.Textures.TextureWraps;

// Uses code from XIVInstantMessager, see link:
// https://github.com/NightmareXIV/XIVInstantMessenger/tree/master/Messenger/Services/EmojiLoaderService
// Helps save me the headache of figuring out how to display gifs in chat.

namespace CkCommons.RichText.Emoji;

/// <summary>
///   Stores the data for a single frame of a potentially animated image. <br/>
///   If none is provided, we can assume it is static and delay is 0.
/// </summary>
public sealed class EmoteFrameData(IDalamudTextureWrap texture, int delayMS) : IDisposable
{
    public IDalamudTextureWrap Texture { get; } = texture;
    public int DelayMS { get; } = delayMS;
    
    public void Dispose()
        => Texture?.Dispose();
}
