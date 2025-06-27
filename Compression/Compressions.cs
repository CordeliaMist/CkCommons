using System.IO;
using System.IO.Compression;

namespace CkCommons;
public static class Compressions
{
    /// <summary> Compress a byte array with a prepended version. </summary>
    public static unsafe byte[] Compress(this byte[] data, byte version)
    {
        using MemoryStream compressedStream = new MemoryStream();
        using GZipStream zipStream = new GZipStream(compressedStream, CompressionMode.Compress);
        zipStream.Write(data, 0, data.Length);
        zipStream.Flush();

        byte[] ret = new byte[compressedStream.Length + 1];
        ret[0] = version;
        fixed (byte* ptr1 = compressedStream.GetBuffer(), ptr2 = ret)
        {
            MemCpyUnchecked(ptr2 + 1, ptr1, (int)compressedStream.Length);
        }

        return ret;
    }

    /// <summary> Compress a string with a prepended version. </summary>
    public static byte[] Compress(this string data, byte version)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(data);
        return bytes.Compress(version);
    }

    /// <summary> Decompress a byte array into a returned version byte and an array of the remaining bytes. </summary>
    public static byte Decompress(this byte[] compressed, out byte[] decompressed)
    {
        byte ret = compressed[0];
        using MemoryStream compressedStream = new MemoryStream(compressed, 1, compressed.Length - 1);
        using GZipStream zipStream = new GZipStream(compressedStream, CompressionMode.Decompress);
        using MemoryStream resultStream = new MemoryStream();
        zipStream.CopyTo(resultStream);
        decompressed = resultStream.ToArray();
        return ret;
    }

    /// <summary> Decompress a byte array into a returned version byte and a string of the remaining bytes as UTF8. </summary>
    public static byte DecompressToString(this byte[] compressed, out string decompressed)
    {
        byte ret = compressed.Decompress(out byte[]? bytes);
        decompressed = Encoding.UTF8.GetString(bytes);
        return ret;
    }

    /// <summary> Copies <paramref name="count"/> bytes from <paramref name="src"/> to <paramref name="dest"/>. </summary>
    public static unsafe void MemCpyUnchecked(void* dest, void* src, int count)
    {
        Span<byte> span = new Span<byte>(src, count);
        Span<byte> target = new Span<byte>(dest, count);
        span.CopyTo(target);
    }
}
