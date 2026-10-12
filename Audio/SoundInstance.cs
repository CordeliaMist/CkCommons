using NAudio.Wave;

namespace CkCommons.Audio;

public class SoundInstance
{
    private readonly CachedSound _sound;
    private readonly LoopingSampleProvider _loopSource;
    private readonly RampedVolumeProvider _volume;

    private double _intensity;

    public bool IsStopped => _loopSource.HasCompleted;
    public ISampleProvider Output => _volume;

    public double Intensity
    {
        get => _intensity;
        set
        {
            if (Math.Abs(_intensity - value) < 0.001)
                return;

            _intensity = Math.Clamp(value, 0.0, 1.0);
            _volume.Volume = (float)_intensity;
        }
    }

    public SoundInstance(CachedSound sound, bool loop, double initialIntensity = 0.0, long startPosition = 0)
    {
        _sound = sound;
        _loopSource = new LoopingSampleProvider(sound, loop, startPosition);
        _volume = new RampedVolumeProvider(_loopSource)
        {
            Volume = (float)Math.Clamp(initialIntensity, 0.0, 1.0)
        };
        _intensity = initialIntensity;
    }

    public void Stop()
        => _loopSource.Stop();

    //// Updates volume based on where another player is from us in distance. (Dont add this yet, maybe in a different class)
    //public void Update(Vector3 soundSourcePos)
    //{
    //    var distance = soundSourcePos == Vector3.Zero ? 0 : Vector3.Distance(soundSourcePos, ClientPos);
    //    var falloff = 1f / (1f + distance * 0.1f); // Maybe make the 0.1 adjustable or something?

    //    _volume.Volume = (float)Math.Clamp(Intensity * falloff, 0f, 1f);
    //    //_panner.Pan = Math.Clamp(ClientPos.X / 10f, -1f, 1f); // Adjust pan based on position.
    //}
}
