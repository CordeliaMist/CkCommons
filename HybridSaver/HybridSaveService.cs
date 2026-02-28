using Dalamud.Plugin.Services;
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

    public void Init()
    {
        _ = Task.Run(async () =>
        {
            while (!_cts.Token.IsCancellationRequested)
            {
                try
                {
                    await CheckDirtyConfigs();
                    await Task.Delay(2000, _cts.Token); // Wait for 2 seconds before checking again
                }
                catch (TaskCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Svc.Log.Error(ex, "[SaveService] Error while checking dirty configs.");
                }
            }
        }, _cts.Token);
    }

    public async Task Dispose()
    {
        // wait for the save lock to finish, then cancel the cts and exit.
        await _saveLock.WaitAsync().ConfigureAwait(false);
        await _cts.CancelAsync().ConfigureAwait(false);
        _cts.Dispose();
    }

    public void Save(IHybridConfig<T> config)
    {
        _saveLock.Wait();
        _dirtyConfigs.Add(config);
        //_logger.LogDebug($"Config {config.GetType().Name} marked as dirty.");
        _saveLock.Release();
    }

    private async Task CheckDirtyConfigs()
    {
        if (_dirtyConfigs.Count == 0)
            return;

        // _logger.LogDebug("Checking for dirty configs.");
        // await for the current semaphore to be released.
        await _saveLock.WaitAsync().ConfigureAwait(false);
        var configs = _dirtyConfigs.ToList();
        _dirtyConfigs.Clear();
        _saveLock.Release();

        // Process each config
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
}
