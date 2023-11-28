namespace DanmakuEngine.Allocations;

public partial class DoubleBuffer<T> : MultiBuffer<T>
{
    private const int buffer_count = 2;

    private readonly Usage<T>[] _buffers = new Usage<T>[buffer_count];

    protected override int bufferCount => buffer_count;

    protected override Usage<T>[] buffers => this._buffers;

    public DoubleBuffer() : base()
    {
    }
}