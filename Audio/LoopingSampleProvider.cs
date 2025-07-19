//using NAudio.Wave;

//namespace CkCommons.Audio;

///// <summary>
/////     ISample provider with built in loop support.
///// </summary>
//public class LoopingSampleProvider : ISampleProvider
//{
//    private readonly float[] _audioData;
//    private readonly WaveFormat _waveFormat;
//    private long _position;
//    private bool _stopped = false;
//    private readonly bool _loop;

//    public LoopingSampleProvider(CachedSound cachedSound, bool loop)
//    {
//        _audioData = cachedSound.AudioData;
//        _waveFormat = cachedSound.WaveFormat;
//        _loop = loop;
//    }

//    public bool HasCompleted => _stopped;

//    public void Stop() => _stopped = true;

//    public int Read(float[] buffer, int offset, int count)
//    {
//        if (_stopped)
//            return 0;

//        var samplesWritten = 0;
//        while (samplesWritten < count)
//        {
//            var available = _audioData.Length - (int)_position;
//            var toCopy = Math.Min(available, count - samplesWritten);

//            Array.Copy(_audioData, _position, buffer, offset + samplesWritten, toCopy);
//            _position += toCopy;
//            samplesWritten += toCopy;
            
//            if (_position >= _audioData.Length)
//            {
//                if (_loop)
//                    _position = 0;
//                else
//                {
//                    _stopped = true; // Stop reading if not looping.
//                    break;
//                }
//            }
//        }
//        return samplesWritten;
//    }

//    public WaveFormat WaveFormat => _waveFormat;
//}
