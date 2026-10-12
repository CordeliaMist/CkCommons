using NAudio.Wave;

namespace CkCommons.Audio;

/// <summary>
///   Smoothes each volume change over ~50ms to avoid clicks and stepping on frequent updates.
/// </summary>
public class RampedVolumeProvider : ISampleProvider
{
    private readonly ISampleProvider _source;
    private readonly int _rampFrames;
    private float _current;
    private float _step;
    private float _rampTarget;
    private volatile float _target;

    public RampedVolumeProvider(ISampleProvider source)
    {
        _source = source;
        _rampFrames = (int)(0.05f * source.WaveFormat.SampleRate);
    }

    public WaveFormat WaveFormat => _source.WaveFormat;

    public float Volume { set => _target = value; }

    public int Read(float[] buffer, int offset, int count)
    {
        var read = _source.Read(buffer, offset, count);
        var channels = WaveFormat.Channels;
        var target = _target;
        if (target != _rampTarget)
        {
            _rampTarget = target;
            _step = (target - _current) / _rampFrames;
        }
        for (var i = 0; i < read; i += channels)
        {
            _current = _step > 0 ? Math.Min(_current + _step, _rampTarget) : Math.Max(_current + _step, _rampTarget);
            for (var c = 0; c < channels; c++)
                buffer[offset + i + c] *= _current;
        }
        return read;
    }
}
