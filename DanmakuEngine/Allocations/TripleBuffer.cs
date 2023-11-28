namespace DanmakuEngine.Allocations;

public partial class TripleBuffer<T> : MultiBuffer<T>
{
    private const int buffer_count = 3;

    private readonly Usage<T>[] _buffers = new Usage<T>[buffer_count];

    protected override int bufferCount => buffer_count;

    protected override Usage<T>[] buffers => this._buffers;

    public TripleBuffer() : base()
    {
    }
}