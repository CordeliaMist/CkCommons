//using CkCommons.Textures;
//using Dalamud.Utility;
//using NAudio.CoreAudioApi;
//using NAudio.Wave;
//using NAudio.Wave.SampleProviders;
//using System.IO;

//namespace CkCommons.Audio;

//public enum OutputType
//{
//    DirectSound,
//    Asio,
//    Wasapi
//}

///// <summary>
/////     As much as i would have REALLY PREFERED to work with the native game audio's SCD files, 
/////     nobody else seems to really want to help me uncover how I can make that possible. <para/>
/////     So we are stuck with this shit for now. <para/>
/////     
/////     Spatial Audio will allow various sounds to be played through NAudio, and happen via poximity of distance. <para/>
/////     
/////     This is a temporary implementation for me to fiddle around and test things with.
///// </summary>
///// <remarks> This can occur in mix with other effects through the actual spatial audio system which needs some revisiting. </remarks>
//public static class AudioSystem
//{
//    private static Dictionary<Guid, string> _directOutAudioDevices = []; // ID : Friendly Name
//    private static List<string> _asioAudioDevices = []; // ID
//    private static Dictionary<string, string> _wasapiAudioDevices = []; // ID : Friendly Name

//    private static MixingSampleProvider _mixer = new MixingSampleProvider(WaveFormat.CreateIeeeFloatWaveFormat(44100, 2));
//    private static IWavePlayer _output = new WaveOutEvent();

//    private static readonly Dictionary<string, CachedSound> _soundCache = new();
//    private static ConcurrentDictionary<string, SoundInstance> _activeMixerSounds = new();
    
//    public static IReadOnlyDictionary<Guid, string> DirectSoundAudioDevices => _directOutAudioDevices;
//    public static IReadOnlyList<string> AsioAudioDevices => _asioAudioDevices;
//    public static IReadOnlyDictionary<string, string> WasapiAudioDevices => _wasapiAudioDevices;
//    public static IReadOnlyDictionary<string, SoundInstance> MixerSounds => _activeMixerSounds;

//    internal static void Init()
//    {
//        // Setup the spatial audio sound system.
//        _mixer = new MixingSampleProvider(WaveFormat.CreateIeeeFloatWaveFormat(44100, 2));
//        // Fetch the devices.
//        FetchLatestAudioDevices();
//        // set the output item.
//        _output = new DirectSoundOut();
//        _output.Init(_mixer);
//        _output.Play();
//    }

//    internal static void Dispose()
//    {
//        foreach (var sound in _activeMixerSounds.Values)
//        {
//            sound.Stop();
//            _mixer.RemoveMixerInput(sound.Output);
//        }
//        _soundCache.Clear();

//        var remaining = _mixer.MixerInputs.Count();
//        Svc.Log.Verbose($"[AudioSystem] Disposed and cleaned up, with {remaining} active mixer inputs.");
//        if (remaining > 0)
//        {
//            Svc.Log.Warning($"[AudioSystem] There are still {remaining} active mixer inputs remaining after disposal." +
//                $"Cleaning them up, but this really shouldn't happen!.");
//            _mixer.RemoveAllMixerInputs();
//        }
//        _output?.Stop();
//        _output?.Dispose();
//    }

//    public static void PlayMixer() => _output.Play();
//    public static void PauseMixer() => _output.Pause();
//    public static void StopMixer() => _output.Stop();

//    public static void CleanupMixer()
//    {
//        foreach (var (key, sound) in _activeMixerSounds)
//        {
//            if (sound.IsStopped)
//            {
//                _mixer.RemoveMixerInput(sound.Output);
//                _activeMixerSounds.TryRemove(key, out _);
//                Svc.Log.Verbose($"Removed disposed sound: {key}");
//            }
//        }
//    }

//    public static CachedSound GetOrAddSound(string file)
//    {
//        // Create the cached sound if it does not yet exist.
//        if (!_soundCache.TryGetValue(file, out var sound))
//        {
//            _soundCache[file] = sound = new CachedSound(Path.Combine(TextureManager.AssetFolderPath, "Audio", file));
//            Svc.Log.Verbose($"[AudioSystem] Loaded sound: {file} ({sound.WaveFormat.SampleRate}Hz, {sound.WaveFormat.Channels} channels)");
//        }
//        return sound;
//    }

//    /// <summary>
//    ///     Plays a sound into the audio system mixer, running together with other sounds.
//    /// </summary>
//    /// <remarks>
//    ///     If the sound doesn't loop, it is removed automatically upon finishing. 
//    ///     Otherwise, the looped ones must be stopped.
//    /// </remarks>
//    public static void PlaySound(string soundKey, CachedSound sound, bool loop = true)
//    {
//        // if the key already exist just return.
//        if (_activeMixerSounds.ContainsKey(soundKey))
//            return;

//        // Create a new sound instance and add it to the mixer.
//        var soundInstance = new SoundInstance(sound, loop);
//        _activeMixerSounds.TryAdd(soundKey, soundInstance);
//        _mixer.AddMixerInput(soundInstance.Output);
//        Svc.Log.Verbose($"Playing sound: {sound.WaveFormat.SampleRate}Hz, {sound.WaveFormat.Channels} channels.");
//    }


//    // Audio Device Management.
//    public static void InitializeOutputDevice(OutputType newType, string deviceId)
//    {
//        // cull the old output.
//        _output?.Dispose();
//        Svc.Log.Debug($"Initializing audio device using {newType}");

//        try
//        {
//            // https://github.com/naudio/NAudio/blob/master/Docs/EnumerateOutputDevices.md
//            if (newType is OutputType.DirectSound)
//            {
//                if (Guid.TryParse(deviceId, out Guid guid))
//                {
//                    _output = new DirectSoundOut(guid);
//                }
//                else
//                {
//                    // invalid param was passed in, so just assign default.
//                    _output = new DirectSoundOut();
//                }
//            }
//            else if (newType is OutputType.Asio)
//            {
//                if (string.IsNullOrEmpty(deviceId) || deviceId == "Default")
//                {
//                    _output = new AsioOut();
//                }
//                else
//                {
//                    _output = new AsioOut(deviceId);
//                }
//            }
//            else if (newType is OutputType.Wasapi)
//            {
//                if (string.IsNullOrEmpty(deviceId))
//                    _output = new WasapiOut();
//                else
//                {
//                    _output = new WasapiOut(GetWasapiAudioDevice(deviceId), AudioClientShareMode.Shared, true, 200);
//                }
//            }
//            else
//            {
//                throw new ArgumentOutOfRangeException(nameof(newType), $"Unsupported output type: {newType}");
//            }

//            // reinitialize the mixer.
//            _output.Init(_mixer);
//            _output.Play();
//        }
//        catch (Exception ex)
//        {
//            Svc.Log.Error($"Failed to initialize audio output device: {ex}");
//        }
//    }

//    public static void FetchLatestAudioDevices()
//    {
//        _directOutAudioDevices = new Dictionary<Guid, string> { { Guid.Empty, "Default" } };
//        try
//        {
//            foreach (DirectSoundDeviceInfo? device in DirectSoundOut.Devices)
//                _directOutAudioDevices.TryAdd(device.Guid, device.Description);
//        }
//        catch (Exception ex)
//        {
//            Svc.Log.Warning(ex, "Unable to get DirectSound devices");
//        }

//        _asioAudioDevices = ["Default"];
//        try
//        {
//            foreach (string device in AsioOut.GetDriverNames())
//                _asioAudioDevices.Add(device);
//        }
//        catch (Exception ex)
//        {
//            Svc.Log.Warning(ex, "Unable to get ASIO devices");
//        }

//        _wasapiAudioDevices = new Dictionary<string, string>() { { string.Empty, "Default" } };
//        try
//        {
//            MMDeviceEnumerator enumerator = new();
//            foreach (MMDevice? device in enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active))
//            {
//                if (device is null)
//                    continue;
//                _wasapiAudioDevices.TryAdd(device.ID, $"{device.FriendlyName}");
//            }
//        }
//        catch (Exception ex)
//        {
//            Svc.Log.Warning(ex, "Unable to get WASAPI devices");
//        }
//    }

//    private static MMDevice? GetWasapiAudioDevice(string id)
//    {
//        MMDeviceEnumerator enumerator = new();
//        foreach (MMDevice? device in enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active))
//        {
//            if (device is null)
//                continue;
//            if (device.ID == id)
//                return device;
//        }

//        return null;
//    }
//}
