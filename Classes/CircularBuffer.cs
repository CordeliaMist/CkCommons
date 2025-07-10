namespace CkCommons.Classes;

/// <summary>
///     Circular buffer that maintains a fixed size. <para/>
///     Yoinked from NightmareXIV's ECommons project, as it efficiently helps with datastreaming.
///     https://github.com/NightmareXIV/ECommons/blob/e5c432fcaecb340b246a187bbf174c61318fbd28/ECommons/CircularBuffers/CircularBuffer.cs
/// </summary>
public class CircularBuffer<T> : IEnumerable<T>
{
    private readonly T[] _buffer;
    private int _start;
    private int _end;
    private int _size;

    public CircularBuffer(int capacity)
    : this((uint)capacity, new T[] { })
    { }

    public CircularBuffer(uint capacity)
        : this(capacity, new T[] { })
    { }

    /// <param name='items'>
    ///     Items to fill buffer with. Items length must be less than capacity. <para/>
    ///     Use Skip(x).Take(y).ToArray() to build this argument from enumerable.
    /// </param>
    public CircularBuffer(uint capacity, T[] items)
    {
        if (capacity < 1)
        {
            throw new ArgumentException("Circular buffer cannot have negative or zero capacity.", nameof(capacity));
        }
        if (items is null)
        {
            throw new ArgumentNullException(nameof(items));
        }
        if (items.Length > capacity)
        {
            throw new ArgumentException("Too many items to fit circular buffer", nameof(items));
        }

        _buffer = new T[capacity];

        Array.Copy(items, _buffer, items.Length);
        _size = items.Length;
        _start = 0;
        _end = _size == capacity ? 0 : _size;
    }

    /// <summary> Maximum elements that can fit into the buffer at once. </summary>
    public int Capacity => _buffer.Length;

    /// <summary> If the capacity has been reached, triggering the circular behavior. </summary>
    public bool IsFull => Size == Capacity;

    /// <summary> If the buffer has no elements. <para/>
    public bool IsEmpty => Size == 0;

    /// <summary> The number of items present in the buffer. </summary>
    public int Size => _size;

    /// <summary> First item in the circular buffer. (this[0]) </summary>
    /// <returns> The value of the element of type T at the front of the buffer. </returns>
    /// <exception cref="InvalidOperationException"> When buffer is empty. </exception>
    public T Front() => IsEmpty ? throw new InvalidOperationException("Buffer is Empty!") : _buffer[_start];

    /// <summary> Last item in the circular buffer. (this[Size - 1]) </summary>
    /// <returns> The value of the element of type T at the back of the buffer. </returns>
    /// <exception cref="InvalidOperationException"> When buffer is empty. </exception>"
    public T Back() => IsEmpty ? throw new InvalidOperationException() : _buffer[(_end == 0 ? Capacity : _end) - 1];

    /// <exception cref="IndexOutOfRangeException"> Accessing element greater than <see cref="_size"/> or less than 0.</exception>
    public T this[int index]
    {
        get
        {
            if (IsEmpty)
                throw new IndexOutOfRangeException(string.Format($"Cannot access index {index}. Buffer is empty"));
            
            if (index >= _size)
                throw new IndexOutOfRangeException(string.Format($"Cannot access index {index}. Buffer size is {_size}"));

            var actualIndex = InternalIndex(index);
            return _buffer[actualIndex];
        }
        set
        {
            if (IsEmpty)
                throw new IndexOutOfRangeException(string.Format($"Cannot access index {index}. Buffer is empty"));

            if (index >= _size)
                throw new IndexOutOfRangeException(string.Format($"Cannot access index {index}. Buffer size is {_size}"));
            var actualIndex = InternalIndex(index);
            _buffer[actualIndex] = value;
        }
    }

    /// <exception cref="IndexOutOfRangeException"> Accessing element greater than <see cref="_size"/> or less than 0.</exception>
    public ref T GetRef(int index)
    {
        if (IsEmpty)
            throw new IndexOutOfRangeException($"Cannot access index {index}. Buffer is empty");

        if (index < 0 || index >= _size)
            throw new IndexOutOfRangeException($"Cannot access index {index}. Buffer size is {_size}");

        return ref _buffer[InternalIndex(index)];
    }

    /// <summary> Behves how you expect a stack to push an element to the back of the circular buffer, but smarter. </summary>
    public void PushBack(T item)
    {
        if (IsFull)
        {
            _buffer[_end] = item;
            Increment(ref _end);
            _start = _end;
        }
        else
        {
            _buffer[_end] = item;
            Increment(ref _end);
            ++_size;
        }
    }

    /// <summary> 
    ///     Pushes the item to the front of the buffer. (this[0]) <para/>
    ///     Full buffers will cause element at (this[Size-1]) to be popped, allowing room to fit.
    /// </summary>
    public void PushFront(T item)
    {
        if (IsFull)
        {
            Decrement(ref _start);
            _end = _start;
            _buffer[_start] = item;
        }
        else
        {
            Decrement(ref _start);
            _buffer[_start] = item;
            ++_size;
        }
    }

    /// <summary> Removes element from the back of the buffer. </summary>
    public void PopBack()
    {
        if (IsEmpty) throw new InvalidOperationException("Cannot take elements from an empty buffer.");
        Decrement(ref _end);
        _buffer[_end] = default(T)!;
        --_size;
    }

    /// <summary> Removes the element at the front of the buffer. </summary>
    public void PopFront()
    {
        if (IsEmpty) throw new InvalidOperationException("Cannot take elements from an empty buffer.");
        _buffer[_start] = default(T)!;
        Increment(ref _start);
        --_size;
    }

    /// <summary> Cleans up the buffer, removing all elements. Capacity will remain the same. </summary>
    public void Clear()
    {
        _start = 0;
        _end = 0;
        _size = 0;
        Array.Clear(_buffer, 0, _buffer.Length);
    }

    /// <summary> Converts all elements into an array format for retrieval. 
    /// Copies the buffer contents to an array, according to the logical
    /// contents of the buffer (i.e. independent of the internal 
    /// order/contents)
    /// </summary>
    /// <returns>A new array with a copy of the buffer contents.</returns>
    public T[] ToArray()
    {
        var newArray = new T[Size];
        var newArrayOffset = 0;
        var segments = ToArraySegments();
        foreach (var segment in segments)
        {
            Array.Copy(segment.Array, segment.Offset, newArray, newArrayOffset, segment.Count);
            newArrayOffset += segment.Count;
        }
        return newArray;
    }

    /// <summary>
    ///     Get the contents of the buffer as 2 ArraySegments.
    ///     Respects the logical contents of the buffer, where
    ///     each segment and items in each segment are ordered
    ///     according to insertion.
    ///
    ///     Fast: does not copy the array elements.
    ///     Useful for methods like <c>Send(IList&lt;ArraySegment&lt;Byte&gt;&gt;)</c>.
    /// </summary>
    /// <remarks>Segments may be empty.</remarks>
    /// <returns>An IList with 2 segments corresponding to the buffer content.</returns>
    public IList<ArraySegment<T>> ToArraySegments()
        => new[] { ArrayOne(), ArrayTwo() };

    /// <summary> Returns an enumerator that iterates through this buffer. </summary>
    public IEnumerator<T> GetEnumerator()
    {
        var segments = ToArraySegments();
        foreach (var segment in segments)
        {
            for (var i = 0; i < segment.Count; i++)
            {
                yield return segment.Array[segment.Offset + i];
            }
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();

    /// <summary> Internal helper to increment the index variable. </summary>
    private void Increment(ref int index)
    {
        if (++index == Capacity)
            index = 0;
    }

    /// <summary> Internal helper to decrement the index variable. </summary>
    private void Decrement(ref int index)
    {
        if (index == 0)
            index = Capacity;

        index--;
    }

    /// <summary> Converts the index in the argument to an index in <code>_buffer</code></summary>
    /// <returns> The transformed index. </returns>
    /// <param name='index'> The external index. </param>
    private int InternalIndex(int index)
        => _start + (index < Capacity - _start ? index : index - Capacity);

    // The array is composed by at most two non-contiguous segments, 
    // the next two methods allow easy access to those.
    private ArraySegment<T> ArrayOne()
    {
        if (IsEmpty)
        {
            return new ArraySegment<T>(new T[0]);
        }
        else if (_start < _end)
        {
            return new ArraySegment<T>(_buffer, _start, _end - _start);
        }
        else
        {
            return new ArraySegment<T>(_buffer, _start, _buffer.Length - _start);
        }
    }

    private ArraySegment<T> ArrayTwo()
    {
        if (IsEmpty)
        {
            return new ArraySegment<T>(new T[0]);
        }
        else if (_start < _end)
        {
            return new ArraySegment<T>(_buffer, _end, 0);
        }
        else
        {
            return new ArraySegment<T>(_buffer, 0, _end);
        }
    }
}
