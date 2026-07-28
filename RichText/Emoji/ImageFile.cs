using CkCommons.Classes;
using Dalamud.Interface.Textures.TextureWraps;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using System.IO;

// Uses code from XIVInstantMessager, see link:
// https://github.com/NightmareXIV/XIVInstantMessenger/tree/master/Messenger/Services/EmojiLoaderService
// Helps save me the headache of figuring out how to display gifs in chat.

namespace CkCommons.RichText.Emoji;

internal enum ImageLoadStatus
{
    NotLoaded,
    Loading,
    Loaded
}

/// <summary>
///   A form of image display that stores PNG or GIF data when displaying an image. <br/>
///   This allows for the use of GIDs in chat, along with images.
/// </summary>
public sealed class ImageFile : IDisposable
{
    private readonly SimpleThreadPool _threadpool;

    private int _totalLength = 0;
    private volatile ImageLoadStatus _status = ImageLoadStatus.NotLoaded;
    public ImageFile(SimpleThreadPool threadpool, string fullPath)
    {
        _threadpool = threadpool;
        ImagePath = fullPath;
    }

    /// <summary>
    ///   The FilePath the image is stored at.
    /// </summary>
    public readonly string ImagePath;

    /// <summary>
    ///   All frames used for this image. <br/>
    ///   If a PNG, only 1 is ever used.
    /// </summary>
    public readonly List<EmoteFrameData> Data = [];

    public bool IsReady => _status is ImageLoadStatus.Loaded;

    public void Load()
    {
        try
        {
            // Comment out when not debugging.
            if (CkCommonsHost.LogFilter.HasFlag(CkLogFilter.Emojis))
                Svc.Log.Verbose($"Loading image {ImagePath}");
            var bytes = File.ReadAllBytes(ImagePath);
            var image = Image.Load(bytes);
            // Decode for Gifs.
            if(image.Frames.Count > 1)
            {
                var pngEncoder = new PngEncoder();
                // Comment out when not debugging.
                if (CkCommonsHost.LogFilter.HasFlag(CkLogFilter.Emojis))
                    Svc.Log.Verbose($" Animation detected");
                for(var i = 0; i < image.Frames.Count; i++)
                {
                    var frame = image.Frames.CloneFrame(i);
                    var meta = image.Frames[i].Metadata.GetGifMetadata();
                    // Allocate the framedata to memory stream for each frame with delay from meta.
                    using var frameData = new MemoryStream();
                    frame.Save(frameData, pngEncoder);
                    if (CkCommonsHost.LogFilter.HasFlag(CkLogFilter.Emojis))
                        Svc.Log.Verbose($"  Loading frame {i}");
                    var delay = meta.FrameDelay == 0 ? 5 : meta.FrameDelay;
                    // Compose and add the frame.
                    var img = new EmoteFrameData(Svc.Texture.CreateFromImageAsync(frameData.ToArray()).Result, delay * 10);
                    Data.Add(img);
                    if (CkCommonsHost.LogFilter.HasFlag(CkLogFilter.Emojis))
                        Svc.Log.Verbose($" Texture: {img.Texture} duration: {img.DelayMS}");
                }
                _totalLength = Data.Sum(x => x.DelayMS);
            }
            else
            {
                if (CkCommonsHost.LogFilter.HasFlag(CkLogFilter.Emojis))
                    Svc.Log.Verbose($" Static image detected");
                var img = new EmoteFrameData(Svc.Texture.CreateFromImageAsync(bytes).Result, 0);
                if (CkCommonsHost.LogFilter.HasFlag(CkLogFilter.Emojis))
                    Svc.Log.Verbose($" Texture: {img.Texture}");
                Data.Add(img);
            }
        }
        catch(Exception e)
        {
            Svc.Log.Error($"Failed to load image {ImagePath}: {e}");
        }
        _status = ImageLoadStatus.Loaded;
    }

    public IDalamudTextureWrap? GetWrapOrDefault()
    {
        if (_status is ImageLoadStatus.NotLoaded)
        {
            _status = ImageLoadStatus.Loading;
            _threadpool.Run(Load);
        }
        if (_status is ImageLoadStatus.Loaded)
        {
            // If an image, return the image.
            if(Data.Count is 1)
                return Data[0].Texture;
            // Otherwise, if a GIF, return the correct frame by delay.
            else if(Data.Count > 1)
            {
                var currentDelay = Environment.TickCount64 % _totalLength;
                var pos = 0;
                for(var i = 0; i < Data.Count; i++)
                {
                    pos += Data[i].DelayMS;
                    if(currentDelay < pos)
                        return Data[i].Texture;
                }
            }
        }
        return null;
    }

    public void Dispose()
    {
        foreach(var x in Data)
        {
            x.Texture.Dispose();
        }
    }
}