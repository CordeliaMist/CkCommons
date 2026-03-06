using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace CkCommons.HybridSaver;

/// <summary> The Base Class for the hybrid save service, not wrapped. </summary>
public class HybridSaveServiceBase<T> where T : IConfigFileProvider
{
    private readonly HashSet<IHybridConfig<T>> _dirtyConfigs = [];
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

    public void Save(IHybridConfig<T> config)
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
        List<IHybridConfig<T>> configs;

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


    private void SaveConfigAsync(IHybridConfig<T> config)
    {
        var configPath = config.GetFileName(FileNames, out var uniquePerAccount);

        if (uniquePerAccount && !FileNames.HasValidProfileConfigs)
        {
            Svc.Log.Warning($"[SaveService] UID is null for {configPath}. Not saving.");
            return;
        }

        var directory = Path.GetDirectoryName(configPath)!;
        Directory.CreateDirectory(directory);

        var antiCorruptionPath = $"{configPath}.new";
        try
        {
            // Recover from previous failed save
            if (File.Exists(antiCorruptionPath))
            {
                var saveTo = $"{antiCorruptionPath}.{DateTimeOffset.Now.ToUnixTimeMilliseconds()}";
                Svc.Log.Warning($"Detected unsuccessfully saved file {antiCorruptionPath}: moving to {saveTo}");
                File.Move(antiCorruptionPath, saveTo);
                Svc.Log.Warning($"Success. Please manually check {saveTo} file contents.");
            }
            // Write to antiCorruption file
            WriteTempFile(config, antiCorruptionPath);
            // Backup if nessisary before we attempt to move.
            CreateBackupIfNeeded(configPath);
            // Atomically move to real file after.
            File.Move(antiCorruptionPath, configPath, overwrite: true);
        }
        catch (Exception ex)
        {
            Svc.Log.Error($"[SaveService] Failed to save {configPath}: {ex}");
        }
    }

    private static void WriteTempFile(IHybridConfig<T> config, string fullPath)
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
    private static void CreateBackupIfNeeded(string configPath)
    {
        if (!File.Exists(configPath))
            return;

        var directory = Path.GetDirectoryName(configPath)!;
        var fileName = Path.GetFileName(configPath);

        var bakFiles = Directory.GetFiles(directory, $"{fileName}.bak*")
            .Select(f => new FileInfo(f))
            .OrderByDescending(f => f.LastWriteTimeUtc)
            .ToList();

        // Check if we should create a new backup
        if (bakFiles.Count > 0)
        {
            var newest = bakFiles[0];
            if (DateTime.UtcNow - newest.LastWriteTimeUtc < TimeSpan.FromHours(2))
                return;
        }

        var backupPath = Path.Combine(directory, $"{fileName}.bak{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

        File.Copy(configPath, backupPath, overwrite: true);

        // Refresh list and trim to 2 backups
        bakFiles = Directory.GetFiles(directory, $"{fileName}.bak*")
            .Select(f => new FileInfo(f))
            .OrderByDescending(f => f.LastWriteTimeUtc)
            .ToList();

        for (int i = 2; i < bakFiles.Count; i++)
        {
            try
            {
                bakFiles[i].Delete();
            }
            catch { }
        }
    }
}
