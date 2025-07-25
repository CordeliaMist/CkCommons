using CkCommons;
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
        // define a temporary filepath.
        var temp = configPath + ".tmp";

        try
        {
            switch (config.SaveType)
            {
                // If JSON, perform the File.WriteAllText method, with a serialize overload.
                case HybridSaveType.Json:
                    var json = config.JsonSerialize();
                    File.WriteAllText(temp, json);
                    break;
                // If StreamWrite, perform the StreamWriter method, with a write overload.
                case HybridSaveType.StreamWrite:
                    var file = new FileInfo(temp);
                    file.Directory?.Create();
                    using (var s = file.Exists ? file.Open(FileMode.Truncate) : file.Open(FileMode.CreateNew))
                    {
                        using var w = new StreamWriter(s, Encoding.UTF8);
                        config.WriteToStream(w);
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            File.Move(temp, configPath, true);
        }
        catch (Exception ex)
        {
            Svc.Log.Error($"[SaveService] Failed to save {configPath}: {ex}");
        }
    }
}
