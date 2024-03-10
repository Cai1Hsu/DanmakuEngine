using System.Collections;

namespace DanmakuEngine.Allocations;

public partial class RingPool<T> : IEnumerable<T>
{
    private readonly int _size;

    private int _index;

    private int filled_count = 0;

    public int FilledCount => filled_count;

    public bool FullFilled => filled_count == _size;

    private readonly T[] _pool;

    public int Size => _size;

    public int Index
    {
        get => _index;
        private set
        {
            _index = value % _size;

            filled_count = Math.Max(filled_count, _index + 1);
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

    // FIXME: This should not be a property
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

    public T this[int index]
    {
        get
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, _size, nameof(index));
            ArgumentOutOfRangeException.ThrowIfNegative(index, nameof(index));

            return _pool[(_index // start at current index
                 + index) % _size];
        }
        set
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, _size, nameof(index));
            ArgumentOutOfRangeException.ThrowIfNegative(index, nameof(index));

            _pool[(_index // start at current index
                 + index) % _size] = value;
        }
    }

    public void Clear()
    {
        Array.Clear(_pool, 0, _size);

        filled_count = 0;
    }

    public void Reset()
    {
        Clear();

        _index = 0;
    }

    public IEnumerator<T> GetEnumerator()
        => new RingPoolEnumerator<T>(this);

    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();
}
