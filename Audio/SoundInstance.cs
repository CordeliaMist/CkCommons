//using NAudio.Wave;
//using NAudio.Wave.SampleProviders;

//namespace CkCommons.Audio;

//public class SoundInstance
//{
//    private readonly CachedSound _sound;
//    private readonly LoopingSampleProvider _loopSource;
//    private readonly VolumeSampleProvider _volume;

//    private double _intensity;

//    public bool IsStopped => _loopSource.HasCompleted;
//    public ISampleProvider Output => _volume;
//    public Vector3 ClientPos => PlayerData.Position;
//    public double Intensity
//    {
//        get => _intensity;
//        set
//        {
//            if (Intensity == value)
//                return;
//            _intensity = value;
//            _volume.Volume = (float)Math.Clamp(_intensity, 0.0, 1.0);
//        }
//    }

//    public SoundInstance(CachedSound sound, bool loop)
//    {
//        _sound = sound;

//        _loopSource = new LoopingSampleProvider(sound, loop);
//        _volume = new VolumeSampleProvider(_loopSource);
//    }

//    // Updates volume based on where another player is from us in distance.
//    public void Update(Vector3 soundSourcePos)
//    {
//        var distance = soundSourcePos == Vector3.Zero ? 0 : Vector3.Distance(soundSourcePos, ClientPos);
//        var falloff = 1f / (1f + distance * 0.1f); // Maybe make the 0.1 adjustable or something?

//        _volume.Volume = (float)Math.Clamp(Intensity * falloff, 0f, 1f);
//        //_panner.Pan = Math.Clamp(ClientPos.X / 10f, -1f, 1f); // Adjust pan based on position.
//    }

//    public void Stop()
//        => _loopSource.Stop();
//}
