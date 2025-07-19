//using NAudio.Wave;

//namespace CkCommons.Audio;

///// <summary>
/////     Cache a sound internally.
///// </summary>
//public class CachedSound
//{
//    public float[] AudioData { get; }
//    public WaveFormat WaveFormat { get; }

//    public CachedSound(string filePath)
//    {
//        // read the audio file stream.
//        using var reader = new AudioFileReader(filePath);
//        WaveFormat = reader.WaveFormat;

//        // obtain the buffer size based on the sample rate and channels.
//        var data = new List<float>();
//        var buffer = new float[reader.WaveFormat.SampleRate * reader.WaveFormat.Channels];

//        int bytesRead;
//        while ((bytesRead = reader.Read(buffer, 0, buffer.Length)) > 0)
//            data.AddRange(buffer.Take(bytesRead));

//        AudioData = data.ToArray();
//    }
//}
