// Uses code from XIVInstantMessager, see link:
// https://github.com/NightmareXIV/XIVInstantMessenger/tree/master/Messenger/Services/EmojiLoaderService
// Helps save me the headache of figuring out how to display gifs in chat.

using CkCommons.Classes;
using CkCommons.Gui;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;
using System.Net.Http;

namespace CkCommons.RichText.Emoji;

// TODO: Make accessors for emoji and drawing static later!

/// <summary>
///   Stores the loads Emoji's for display. Can be parented.
/// </summary>
public class EmojiLoader : IDisposable
{
    protected readonly SimpleThreadPool _pool;
    protected readonly HttpClient _httpClient;

    private HashSet<string> _prevSearchRequests = [];
    private ConcurrentQueue<string> _searchRequests = [];
    protected Dictionary<string, ImageFile> _cache = new Dictionary<string, ImageFile>(StringComparer.OrdinalIgnoreCase);
    protected bool _downloaderRunning = false;

    public EmojiLoader(SimpleThreadPool threadpool)
    {
        _pool = threadpool;
        _httpClient = new HttpClient() { Timeout = TimeSpan.FromSeconds(10) };
    }

    public bool IsDownloading => _downloaderRunning;

    // Public Accessor.
    public IReadOnlyDictionary<string, ImageFile> Emotes => _cache;

    public virtual void Dispose()
    {
        _httpClient.Dispose();
        foreach (var x in _cache)
            x.Value.Dispose();
        _pool.Dispose();
    }

    // Rework overtime maybe.
    public void Search(string searchString)
    {
        // Append the search to the requests.
        _prevSearchRequests.Add(searchString);
        _searchRequests.Enqueue(searchString);
        // If no task is running, begin.
        if (!_downloaderRunning)
        {
            Svc.Log.Warning("Would have done a lookup here!");
            // RunDownloaderTask();
        }
    }

    public ImageFile? GetEmojiOrDefault(string imageId)
        => _cache.TryGetValue(imageId, out var image) ? image : null;

    public void DrawEmoji(string emojiId, float size)
        => DrawEmoji(emojiId, new Vector2(size));

    public virtual void DrawEmoji(string emojiName, Vector2 size)
    {
        if (GetEmojiOrDefault(emojiName)?.GetWrapOrDefault() is { } wrap)
        {
            ImGui.Image(wrap.Handle, size);
            if (ImGui.IsItemHovered())
            {
                using (ImRaii.Tooltip())
                {
                    ImGui.TextUnformatted($":{emojiName}:");
                    ImGui.Image(wrap.Handle, size * 1.5f);
                }
            }
        }
        else
        {
            ImGui.Dummy(size);
            CkGui.AttachTooltip($":{emojiName}:");
        }
    }
}
