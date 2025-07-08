namespace CkCommons.Classes;

/// <summary>
///     Similar to a circular buffer, except all previous data save a fraction 
///     is cleared each time the threshold is reached, to provide more efficient storage.
/// </summary>
public class RollingBuffer<T>
{
    private readonly int _threshold;
    private readonly double _retainFraction;
    private readonly List<T> _buffer;

    public RollingBuffer(int threshold, double retainFraction = 0.2)
    {
        _threshold = threshold;
        _retainFraction = retainFraction;
        _buffer = new List<T>();
    }

    public int Count => _buffer.Count;
    public T this[int index] => _buffer[index];
    public IReadOnlyList<T> Items => _buffer;

    public void Add(T item)
    {
        _buffer.Add(item);
        if (_buffer.Count > _threshold)
        {
            int retain = (int)(_threshold * _retainFraction);
            if (retain < 1) retain = 1;
            _buffer.RemoveRange(0, _buffer.Count - retain);
        }
    }

    public void Clear() => _buffer.Clear();
}
