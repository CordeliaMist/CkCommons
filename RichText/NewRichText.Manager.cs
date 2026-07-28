using CkCommons.RichText.Emoji;
using System.Threading;
using System.Threading.Tasks;

namespace CkCommons.RichText;

/// <summary>
///   Notoriously in ImGui, textwrapping in combination with other elements is a nightmare. <br/>
///   Because TextWrapPos is based on the starting location, any wrap function 
///   after becomes offset to the start of that wrap, over wrapping to the far left
///   of the available region.
///   <para/>
///   Due to this, RichTextStrings must have their segments pre-calculated and cached
///   upon changes, to ensure CalcTextSize does not need to be made every frame.
/// </summary>
public static partial class NewRichText
{
    internal record RichTextKey(string id, string rawText);

    // Not the best method to cache everything and perform cleanups but
    // it's the best performance benefit i've gotten so far for per-frame drawing.
    // Optimize later if anything better is discovered.
    internal static ConcurrentDictionary<RichTextKey, NewRichString> _cache = new();

    // Monitored Cleanup service.
    private static readonly HashSet<RichTextKey> _accessedKeys = new();
    private static CancellationTokenSource _cleanupCts = new();
    private static Task? _cleanupTask;

    // For Emoji Support Handlers.
    private static bool _emojiSupport = false;
    private static EmojiLoader? _emojiLoader;
    private static string? ImageLookupRootPath;

    public static bool DoLogging { get; private set; } = false;

    // Can be overriden to control how things display.
    public static bool ShowEmojis
    {
        get => _emojiSupport;
        set
        {
            var prev = _emojiSupport;
            _emojiSupport = value;
            if (prev != value)
                ForceCleanCache();
        }
    }

    /// <summary> The class used to resolve emojis. Defaults to NULL. </summary>
    public static EmojiLoader? EmojiLoader
    {
        get => _emojiLoader;
        set
        {
            _emojiLoader = value;
            ForceCleanCache();
        }
    }

    public static string? ImageRootPath
    {
        get => ImageLookupRootPath;
        set
        {
            var prev = ImageLookupRootPath;
            ImageLookupRootPath = value;
            if (prev != value)
                ForceCleanCache();
        }
    }

    public static void ForceCleanCache()
    {
        _cache.Clear();
        _accessedKeys.Clear();
    }

    internal static void Init()
    {
        _cleanupTask = CleanupLoop(_cleanupCts.Token);
    }

    // Still figuring out how to make a desisive choice on this cleanup cache period.
    internal static async Task CleanupLoop(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            // Can vary this based on needs, or make it a modifiable value for external control.
            await Task.Delay(TimeSpan.FromMinutes(1), token).ConfigureAwait(false);
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
