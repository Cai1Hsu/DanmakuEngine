using System.Collections;

namespace DanmakuEngine.Allocations;

public partial class RingPool<T>
{
#pragma warning disable CS0693 // Type parameter has the same name as the type parameter from outer type
    public sealed class RingPoolEnumerator<T>(RingPool<T> pool) : IEnumerator<T>
#pragma warning restore CS0693
    {
        public T Current => _pool[_index];

        object IEnumerator.Current => Current!;

        private readonly RingPool<T> _pool = pool;

        private readonly int _start_index = pool.Index;

        private int _index = -1;
        private int max_index => _pool.Size - 1;

        public bool MoveNext()
        {
            if (_start_index != _pool.Index)
                throw new InvalidOperationException("The pool has been modified during enumeration");

            if (_index == _pool.FilledCount)
                return false;

            if (_index == max_index)
                return false;

            _index++;

            return true;
        }

        public void Reset()
        {
            if (_start_index != _pool.Index)
                throw new InvalidOperationException("The pool has been modified during enumeration");

            _index = -1;
            _pool.filled_count = 0;
        }

        public void Dispose()
        {
        }
    }
}
