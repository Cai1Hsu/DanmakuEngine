namespace DanmakuEngine.Allocations;

public class RingPool<T>
{
    private readonly int _size;

    private int _index;

    private int filled_count = 0;

    public int FilledCount => filled_count;

    private readonly T[] _pool;

    public int Size => _size;

    public int Index
    {
        get => _index;
        private set
        {
            _index = value % _size;

            filled_count = Math.Max(filled_count, _index);
        }
    }

    public RingPool(int size)
    {
        _size = size;
        _pool = new T[size];
    }

    /// <summary>
    /// Gets or sets the current element in the pool.
    /// </summary>
    public T Current
    {
        get => _pool[_index];
        set => _pool[_index] = value;
    }

    public T CurrentAndNext
    {
        get
        {
            var current = _pool[_index];
            Index++;
            return current;
        }
        set
        {
            _pool[_index] = value;
            Index++;
        }
    }

    public IEnumerable<T> Get()
        => Get(FilledCount);

    public IEnumerable<T> Get(int count)
    {
        if (count > _size)
            throw new ArgumentException($"Count cannot be greater than the size of the pool. Size: {_size}, but found: {count}");

        for (var i = 0; i < count; i++)
        {
            yield return CurrentAndNext;
        }
    }
}
