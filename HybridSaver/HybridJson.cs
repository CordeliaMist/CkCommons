using Dalamud.Interface.ImGuiNotification;
using Newtonsoft.Json.Linq;
using System.IO;

namespace CkCommons.HybridSaver;

/// <summary> Loads configs saved with IReliableFileStorage, recovering from corrupted files where possible. </summary>
public static class HybridJson
{
    /// <summary> Parses the config at path, falls back to the IReliableFileStorage backup, then any old .bak files. </summary>
    /// <returns> The parsed JObject, or null if the file does not exist or nothing could be recovered. </returns>
    /// <remarks> If nothing could be recovered, the corrupt file is kept as .corrupt{time} and the user is notified. </remarks>
    public static JObject? Load(string path)
    {
        var fileName = Path.GetFileName(path);
        JObject? jObject = null;
        var reads = 0;
        try
        {
            // Files written by the old saver start with a UTF-8 BOM, which ReliableStorage leaves in the text.
            Svc.ReliableStorage.ReadAllTextAsync(path, text => { reads++; jObject = JObject.Parse(text.TrimStart('\uFEFF')); }).GetAwaiter().GetResult();
            if (reads > 1)
            {
                Svc.Log.Warning($"[HybridJson] {fileName} was corrupt, restored from reliable storage backup.");
                Notify(NotificationType.Warning, $"{fileName} was corrupted and has been restored from a backup.", TimeSpan.FromSeconds(30));
            }
            return jObject;
        }
        catch (FileNotFoundException)
        {
            return null;
        }
        catch (Exception ex)
        {
            Svc.Log.Error($"[HybridJson] {fileName} is corrupt and has no reliable storage backup: {ex.Message}");
        }

        // Fallback to .bak files made before switching to IReliableFileStorage.
        var bakFiles = Directory.GetFiles(Path.GetDirectoryName(path)!, $"{fileName}.bak*")
            .Select(f => new FileInfo(f))
            .OrderByDescending(f => f.LastWriteTimeUtc);
        foreach (var bak in bakFiles)
        {
            try
            {
                jObject = JObject.Parse(File.ReadAllText(bak.FullName));
                Svc.Log.Warning($"[HybridJson] Restored {fileName} from {bak.Name}");
                Notify(NotificationType.Warning, $"{fileName} was corrupted and has been restored from a backup made {bak.LastWriteTime:g}.", TimeSpan.FromSeconds(30));
                return jObject;
            }
            catch
            {
                Svc.Log.Warning($"[HybridJson] Backup {bak.Name} is also corrupt, skipping.");
            }
        }

        var corruptPath = $"{path}.corrupt{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";
        try { File.Move(path, corruptPath, overwrite: true); }
        catch (Exception ex) { Svc.Log.Error($"[HybridJson] Failed to move corrupt {fileName} aside: {ex.Message}"); }

        Svc.Log.Error($"[HybridJson] No valid backup for {fileName}, it will be reset. Corrupt copy kept at {corruptPath}");
        Notify(NotificationType.Error, $"{fileName} was corrupted and no backup could be loaded, so it has been reset.\nThe corrupt copy was kept as {Path.GetFileName(corruptPath)}.", TimeSpan.MaxValue);
        return null;
    }

    private static void Notify(NotificationType type, string content, TimeSpan duration)
        => Svc.Notifications.AddNotification(new Notification()
        {
            Title = "Config File Corrupted",
            Content = content,
            Type = type,
            Minimized = false,
            InitialDuration = duration
        });
}
