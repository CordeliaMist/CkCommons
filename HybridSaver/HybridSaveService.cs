using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace CkCommons.HybridSaver;

/// <summary> The Base Class for the hybrid save service, not wrapped. </summary>
public class HybridSaveServiceBase<T> where T : IConfigFileProvider
{
    private readonly HashSet<IHybridSavable<T>> _dirtyConfigs = [];
    private readonly SemaphoreSlim _saveLock = new(1, 1);
    private readonly CancellationTokenSource _cts = new();

    public readonly T FileNames = default(T)!;
    public HybridSaveServiceBase(T fileNameStructure)
    {
        FileNames = fileNameStructure;
    }

    private Task? _saveLoopTask;

    public void Init()
    {
        _saveLoopTask = Task.Run(async () =>
        {
            while (!_cts.Token.IsCancellationRequested)
            {
                try
                {
                    await FlushDirtyConfigs().ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Svc.Log.Error(ex, "[SaveService] Error flushing dirty configs. Will retry on next tick.");
                }

                try
                {
                    await Task.Delay(2000, _cts.Token).ConfigureAwait(false);
                }
                catch (TaskCanceledException)
                {
                    // expected when stopping
                    break;
                }
            }
        }, _cts.Token);
    }

    public async Task Dispose()
    {
        // Stop the background loop
        await _cts.CancelAsync().ConfigureAwait(false);

        // wait for the loop to exit
        if (_saveLoopTask != null)
        {
            try
            {
                await _saveLoopTask.ConfigureAwait(false);
            }
            catch (TaskCanceledException) { }
        }

        // Flush remaining dirty configs before exiting.
        Svc.Log.Information("Flushing out remaining configs to save before stopping");
        await FlushDirtyConfigs().ConfigureAwait(false);
        _cts.Dispose();
    }

    public void Save(IHybridSavable<T> config)
    {
        if (_cts.IsCancellationRequested)
            return;

        _saveLock.Wait();
        try
        {
            //_logger.LogDebug($"Config {config.GetType().Name} marked as dirty.");
            _dirtyConfigs.Add(config);
        }
        finally
        {
            _saveLock.Release();
        }
    }

    private async Task FlushDirtyConfigs()
    {
        List<IHybridSavable<T>> configs;

        // _logger.LogDebug("Checking for dirty configs.");
        // await for the current semaphore to be released.
        await _saveLock.WaitAsync().ConfigureAwait(false);
        try
        {
            if (_dirtyConfigs.Count == 0)
                return;

            configs = _dirtyConfigs.ToList();
            _dirtyConfigs.Clear();
        }
        finally
        {
            _saveLock.Release();
        }

        // Perform the config saves
        foreach (var config in configs)
            SaveConfigAsync(config);
    }


    private void SaveConfigAsync(IHybridSavable<T> config)
    {
        var configPath = config.ToFilePath(FileNames);

        // This should be handled by the config file provider, not the saver.
        // We dont want to enforce directory creation if it does not exist.
        var directory = Path.GetDirectoryName(configPath)!;
        if (!Directory.Exists(directory))
        {
            Svc.Log.Warning($"[SaveService] Directory did not exist: {directory}. Ensure your fileProvider inheriting this initializes your folders!");
            return;
        }

        // Use a unique anti-corruption file to avoid overwriting a previous failed save.
        var antiCorruptionPath = $"{configPath}.new.{Guid.NewGuid():N}";
        try
        {
            // Write to antiCorruption file
            WriteTempFile(config, antiCorruptionPath);
            // Backup if nessisary before we attempt to move.
            CreateBackupIfNeeded(config, configPath);
            // Atomically move to real file after.
            File.Move(antiCorruptionPath, configPath, overwrite: true);
        }
        catch (Exception ex)
        {
            Svc.Log.Error($"[SaveService] Failed to save {configPath}: {ex}");
        }
        finally
        {
            // Cleanup the antiCorruption file if it still exists.
            if (File.Exists(antiCorruptionPath))
            {
                Svc.Log.Warning($"[SaveService] Cleaning up anti-corruption file {antiCorruptionPath}");
                try { File.Delete(antiCorruptionPath); } catch { }
            }
        }
    }

    private static void WriteTempFile(IHybridSavable<T> config, string fullPath)
    {
        switch (config.SaveType)
        {
            case HybridSaveType.Json:
                {
                    var json = config.JsonSerialize();
                    File.WriteAllText(fullPath, json, Encoding.UTF8);
                    break;
                }
            case HybridSaveType.StreamWrite:
                {
                    using var fs = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None);
                    using var writer = new StreamWriter(fs, Encoding.UTF8);
                    config.WriteToStream(writer);
                    break;
                }
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    // Temp solution until we migrate to IReliableStorage.
    private static void CreateBackupIfNeeded(IHybridSavable<T> config, string configPath)
    {
        if (!File.Exists(configPath))
            return;

        var directory = Path.GetDirectoryName(configPath)!;
        var fileName = Path.GetFileName(configPath);

        var bakFiles = Directory.GetFiles(directory, $"{fileName}.bak*")
            .Select(f => new FileInfo(f))
            .OrderByDescending(f => f.LastWriteTimeUtc)
            .ToList();

        // if 0, cleanup any orphaned backups.
        if (config.MaxBackups <= 0)
        {
            foreach (var file in bakFiles)
                try { file.Delete(); } catch { }
            return;
        }

        // Determine if backup is needed
        var needsBackup = true;
        if (bakFiles.Count > 0)
        {
            var newest = bakFiles[0];
            if (DateTime.UtcNow - newest.LastWriteTimeUtc < TimeSpan.FromHours(2))
                needsBackup = false;
        }

        // Create the backup if required, and track it in our existing list
        if (needsBackup)
        {
            var backupPath = Path.Combine(directory, $"{fileName}.bak{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");
            File.Copy(configPath, backupPath, overwrite: true);
            // Insert at the top of the list since it's the newest, avoiding a second disk read
            bakFiles.Insert(0, new FileInfo(backupPath));
        }

        // Populate a unified cleanup loop enforces the MaxBackups limit
        for (int i = config.MaxBackups; i < bakFiles.Count; i++)
            try { bakFiles[i].Delete(); } catch { }
    }
}
