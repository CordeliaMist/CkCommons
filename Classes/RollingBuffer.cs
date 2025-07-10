namespace CkCommons.Classes;

/// <summary>
///     Similar to a circular buffer, except all previous data save a fraction 
///     is cleared each time the threshold is reached, to provide more efficient storage.
/// </summary>
/// <remarks> Values can be referenced due to it being array. </remarks>
public class RollingBuffer<T>
{
    private readonly int _threshold;
    private readonly double _retainFraction;
    private T[] _buffer;
    private int _count;

    public RollingBuffer(int threshold, double retainFraction = 0.2)
    {
        _threshold = threshold;
        _retainFraction = retainFraction;
        _buffer = new T[threshold];
        _count = 0;
    }

    public int Count => _count;
    public ref T this[int index] => ref _buffer[index];
    public IReadOnlyList<T> Items => _buffer;

    public void PushBack(T item)
    {
        if (_count < _threshold)
        {
            _buffer[_count++] = item;
        }
        else
        {
            int retain = (int)(_threshold * _retainFraction);
            if (retain < 1) retain = 1;
            // Shift retained items to the front
            Array.Copy(_buffer, _count - retain, _buffer, 0, retain);
            _count = retain;
            _buffer[_count++] = item;
        }
    }

    public void PushFront(T item)
    {
        if (_count < _threshold)
        {
            // Shift existing elements one position right to free up index 0
            if (_count > 0)
                Array.Copy(_buffer, 0, _buffer, 1, _count);

            _buffer[0] = item;
            _count++;
        }
        else
        {
            int retain = (int)(_threshold * _retainFraction);
            if (retain < 1) retain = 1;

            _count = retain;
            // Shift retained elements to the right to free index 0 for new item
            Array.Copy(_buffer, 0, _buffer, 1, retain);

            _buffer[0] = item;
            _count++;
        }
    }

    public void Clear()
    {
        Array.Clear(_buffer, 0, _count);
        _count = 0;
    }
}
