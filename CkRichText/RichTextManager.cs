using CkCommons;
using CkCommons.Textures;
using Dalamud.Interface.Textures.TextureWraps;
using System.Threading;
using System.Threading.Tasks;

namespace CkCommons.RichText;

/// <summary>
///     A class dedicated to mimicing the structure of dalamuds SeString composition but for ImGui. <para/>
///     CkRichText allows for composed strings of colors, normal text, outlined text, images, and emotes. <para/>
///     Supports proper text wrapping, with internal caching for optimal drawtime performance.
/// </summary>
public static partial class CkRichText
{
    /// <summary> Represents a key for caching rich text strings. </summary>
    /// <remarks> The <paramref name="cloneId"/> helps stop rapid caching if in multiple windows. </remarks>
    internal record RichTextKey(int cloneId, string rawText);

    /// <summary> Cache for RichTextStrings to avoid re-creating them if already cached. </summary>
    internal static ConcurrentDictionary<RichTextKey, RichTextString> _cache = new();

    /// <summary> Resolves the emote texture wrap. Must be manually assigned to. </summary>
    /// <remarks> If left unassigned, it will attempt to look for an image in an Asset Folder. </remarks>
    public static Func<string, IDalamudTextureWrap?> EmoteResolver = TextureManager.AssetImageOrDefault;

    // Monitored Cleanup service.
    private static readonly HashSet<RichTextKey> _accessedKeys = new();
    private static CancellationTokenSource _cleanupCts = new();
    private static Task? _cleanupTask;

    public static bool DoLogging { get; private set; } = false;

    public static void DefineEmoteResolver(Func<string, IDalamudTextureWrap?> resolver)
    {
        EmoteResolver = resolver;
    }

    public static void ForceCleanCache()
    {
        _cache.Clear();
        _accessedKeys.Clear();
    }

    internal static void Init(bool doLogging)
    {
        // Token should be already made.
        DoLogging = doLogging;
        _cleanupTask = CleanupLoop(_cleanupCts.Token);
    }

    // Still figuring out how to make a desisive choice on this cleanup cache period.
    internal static async Task CleanupLoop(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromMinutes(5), token).ConfigureAwait(false);
            var accessed = _accessedKeys.ToHashSet();
            foreach (var key in _cache.Keys)
            {
                if (!accessed.Contains(key))
                    _cache.TryRemove(key, out _);
            }
        }
    }

    internal static void Dispose()
    {
        Svc.Log.Information("[CkRichText] Disposing of RichText Cache.");
        _cleanupCts?.SafeCancel();
        try
        {
            _cleanupTask?.Wait();
        }
        catch (AggregateException ex) when (ex.InnerException is TaskCanceledException)
        {
            // Expected during shutdown, ignore
        }
        catch (TaskCanceledException)
        {
            // Expected during shutdown, ignore
        }
        _cleanupTask = null;
        _cleanupCts?.SafeDispose();
        _cache.Clear();
        _accessedKeys.Clear();
    }
}
