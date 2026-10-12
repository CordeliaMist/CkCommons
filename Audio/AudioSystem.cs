using CkCommons.Textures;
using Dalamud.Utility;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System.IO;
using static FFXIVClientStructs.FFXIV.Client.Game.FashionCheckManager.Delegates;
using static FFXIVClientStructs.FFXIV.Client.System.Input.KeyboardDevice.Delegates;

namespace CkCommons.Audio;

public enum OutputType
{
    DirectSound,
    Asio,
    Wasapi
}

/// <summary>
///   This is very much BETA and experimental, use at your own risk as it is developed.
/// </summary>
public static class AudioSystem
{
    private static MixingSampleProvider _mixer = new(WaveFormat.CreateIeeeFloatWaveFormat(44100, 2)) { ReadFully = true };
    private static IWavePlayer _output = new DirectSoundOut();
    private const int Latency = 150; // ms, default 40 underruns (crackles) under load. Maybe make this a config option instead?

    private static readonly ConcurrentDictionary<string, CachedSound> _soundCache = new();
    private static readonly ConcurrentDictionary<string, SoundInstance> _activeMixerSounds = new();

    // Public Accessors
    public static Dictionary<Guid, string> DirectSoundAudioDevices { get; private set; } = [];
    public static List<string> AsioAudioDevices { get; private set; } = [];
    public static Dictionary<string, string> WasapiAudioDevices { get; private set; } = [];
    public static IReadOnlyDictionary<string, SoundInstance> MixerSounds => _activeMixerSounds;

    internal static void Init()
    {
        // Setup the spatial audio sound system.
        _mixer = new MixingSampleProvider(WaveFormat.CreateIeeeFloatWaveFormat(44100, 2)) { ReadFully = true };
        FetchLatestAudioDevices();

        // set the output item.
        _output = new DirectSoundOut(Latency);
        _output.PlaybackStopped += OnPlaybackStopped;
        _output.Init(_mixer);
        _output.Play();
    }

    internal static void Dispose()
    {
        StopAllSounds();
        _soundCache.Clear();

        _output.PlaybackStopped -= OnPlaybackStopped;
        _output?.Stop();
        _output?.Dispose();
    }

    // Output halts for good on underrun / device errors, so restart it.
    private static void OnPlaybackStopped(object? sender, StoppedEventArgs e)
    {
        if (e.Exception is not null)
            Svc.Log.Warning($"Audio output stopped: {e.Exception}");
        _output.Play();
    }

    public static void PlayMixer()
        => _output.Play();
    
    public static void PauseMixer()
        => _output.Pause();
    
    public static void StopMixer()
        => _output.Stop();

    public static void CleanupMixer()
    {
        foreach (var (key, sound) in _activeMixerSounds)
        {
            if (sound.IsStopped)
            {
                _mixer.RemoveMixerInput(sound.Output);
                _activeMixerSounds.TryRemove(key, out _);
                Svc.Log.Verbose($"Removed disposed sound: {key}");
            }
        }
    }

    public static CachedSound GetOrAddSound(string file)
    {
        return _soundCache.GetOrAdd(file, f =>
        {
            var sound = new CachedSound(f);
            Svc.Log.Verbose($"[AudioSystem] Loaded sound: {f} ({sound.WaveFormat.SampleRate}Hz)");
            return sound;
        });
    }

    /// <summary>
    ///   Starts playing a sound into the mixer. If it's a rotor, initialIntensity allows fading it in from 0.
    /// </summary>
    public static void PlaySound(string soundKey, CachedSound sound, bool loop = true, double initialIntensity = 1.0, long startPosition = 0)
    {
        if (_activeMixerSounds.ContainsKey(soundKey))
        {
            Svc.Log.Verbose($"Sound {soundKey} is already playing, skipping.");
            return;
        }
        var soundInstance = new SoundInstance(sound, loop, initialIntensity, startPosition);
        if (_activeMixerSounds.TryAdd(soundKey, soundInstance))
        {
            _mixer.AddMixerInput(soundInstance.Output);
            Svc.Log.Verbose($"Playing sound: {soundKey}.");
        }
    }

    /// <summary>
    ///   Directly controls the volume of a specific running motor/sound.
    /// </summary>
    public static void SetIntensity(string soundKey, double intensity)
    {
        if (_activeMixerSounds.TryGetValue(soundKey, out var instance))
            instance.Intensity = intensity;
    }

    public static void StopSound(string soundKey)
    {
        if (_activeMixerSounds.TryGetValue(soundKey, out var instance))
        {
            instance.Stop();
            // Free key so the sound can be registered again later
            _activeMixerSounds.TryRemove(soundKey, out _);
            Svc.Log.Verbose($"Stopped sound: {soundKey}.");
        }
    }

    public static void StopAllSounds()
    {
        foreach (var sound in _activeMixerSounds.Values)
        {
            sound.Stop();
            _mixer.RemoveMixerInput(sound.Output);
        }
        _activeMixerSounds.Clear();
    }

    public static void InitializeOutputDevice(OutputType newType, string deviceId)
    {
        _output.PlaybackStopped -= OnPlaybackStopped;
        _output?.Dispose();
        Svc.Log.Debug($"Initializing audio device using {newType}");

        try
        {
            _output = newType switch
            {
                OutputType.DirectSound => Guid.TryParse(deviceId, out Guid guid)
                    ? new DirectSoundOut(guid, Latency)
                    : new DirectSoundOut(Latency),

                OutputType.Asio => (string.IsNullOrEmpty(deviceId) || deviceId == "Default")
                    ? new AsioOut()
                    : new AsioOut(deviceId),

                OutputType.Wasapi => string.IsNullOrEmpty(deviceId)
                    ? new WasapiOut()
                    : new WasapiOut(GetWasapiAudioDevice(deviceId), AudioClientShareMode.Shared, true, 200),

                _ => throw new ArgumentOutOfRangeException(nameof(newType), $"Unsupported output type: {newType}")
            };

            _output.PlaybackStopped += OnPlaybackStopped;
            _output.Init(_mixer);
            _output.Play();
        }
        catch (Exception ex)
        {
            Svc.Log.Error($"Failed to initialize audio output device: {ex}");
        }
    }

    public static void FetchLatestAudioDevices()
    {
        DirectSoundAudioDevices = new Dictionary<Guid, string> { { Guid.Empty, "Default" } };
        try
        {
            foreach (var device in DirectSoundOut.Devices)
                DirectSoundAudioDevices.TryAdd(device.Guid, device.Description);
        }
        catch
        {
            Svc.Log.Warning("Unable to get DirectSound devices");
        }

        AsioAudioDevices = ["Default"];
        try
        {
            AsioAudioDevices.AddRange(AsioOut.GetDriverNames());
        }
        catch
        {
            Svc.Log.Warning("Unable to get ASIO devices");
        }

        WasapiAudioDevices = new Dictionary<string, string> { { string.Empty, "Default" } };
        try
        {
            var enumerator = new MMDeviceEnumerator();
            foreach (var device in enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active))
            {
                if (device != null)
                    WasapiAudioDevices.TryAdd(device.ID, device.FriendlyName);
            }
        }
        catch
        {
            Svc.Log.Warning("Unable to get WASAPI devices");
        }
    }

    private static MMDevice? GetWasapiAudioDevice(string id)
    {
        var enumerator = new MMDeviceEnumerator();
        return enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active).FirstOrDefault(device => device?.ID == id);
    }


    //// Audio Device Management.
    //public static void InitializeOutputDevice(OutputType newType, string deviceId)
    //{
    //    // cull the old output.
    //    _output?.Dispose();
    //    Svc.Log.Debug($"Initializing audio device using {newType}");

    //    try
    //    {
    //        // https://github.com/naudio/NAudio/blob/master/Docs/EnumerateOutputDevices.md
    //        if (newType is OutputType.DirectSound)
    //        {
    //            if (Guid.TryParse(deviceId, out Guid guid))
    //            {
    //                _output = new DirectSoundOut(guid);
    //            }
    //            else
    //            {
    //                // invalid param was passed in, so just assign default.
    //                _output = new DirectSoundOut();
    //            }
    //        }
    //        else if (newType is OutputType.Asio)
    //        {
    //            if (string.IsNullOrEmpty(deviceId) || deviceId == "Default")
    //            {
    //                _output = new AsioOut();
    //            }
    //            else
    //            {
    //                _output = new AsioOut(deviceId);
    //            }
    //        }
    //        else if (newType is OutputType.Wasapi)
    //        {
    //            if (string.IsNullOrEmpty(deviceId))
    //                _output = new WasapiOut();
    //            else
    //            {
    //                _output = new WasapiOut(GetWasapiAudioDevice(deviceId), AudioClientShareMode.Shared, true, 200);
    //            }
    //        }
    //        else
    //        {
    //            throw new ArgumentOutOfRangeException(nameof(newType), $"Unsupported output type: {newType}");
    //        }

    //        // reinitialize the mixer.
    //        _output.Init(_mixer);
    //        _output.Play();
    //    }
    //    catch (Exception ex)
    //    {
    //        Svc.Log.Error($"Failed to initialize audio output device: {ex}");
    //    }
    //}

    //public static void FetchLatestAudioDevices()
    //{
    //    _directOutAudioDevices = new Dictionary<Guid, string> { { Guid.Empty, "Default" } };
    //    try
    //    {
    //        foreach (DirectSoundDeviceInfo? device in DirectSoundOut.Devices)
    //            _directOutAudioDevices.TryAdd(device.Guid, device.Description);
    //    }
    //    catch (Exception ex)
    //    {
    //        Svc.Log.Warning(ex, "Unable to get DirectSound devices");
    //    }

    //    _asioAudioDevices = ["Default"];
    //    try
    //    {
    //        foreach (string device in AsioOut.GetDriverNames())
    //            _asioAudioDevices.Add(device);
    //    }
    //    catch (Exception ex)
    //    {
    //        Svc.Log.Warning(ex, "Unable to get ASIO devices");
    //    }

    //    _wasapiAudioDevices = new Dictionary<string, string>() { { string.Empty, "Default" } };
    //    try
    //    {
    //        MMDeviceEnumerator enumerator = new();
    //        foreach (MMDevice? device in enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active))
    //        {
    //            if (device is null)
    //                continue;
    //            _wasapiAudioDevices.TryAdd(device.ID, $"{device.FriendlyName}");
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        Svc.Log.Warning(ex, "Unable to get WASAPI devices");
    //    }
    //}

    //private static MMDevice? GetWasapiAudioDevice(string id)
    //{
    //    MMDeviceEnumerator enumerator = new();
    //    foreach (MMDevice? device in enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active))
    //    {
    //        if (device is null)
    //            continue;
    //        if (device.ID == id)
    //            return device;
    //    }

    //    return null;
    //}
}
