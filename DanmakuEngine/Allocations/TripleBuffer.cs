namespace DanmakuEngine.Allocations;

public partial class TripleBuffer<T> : MultiBuffer<T>
{
    private const int buffer_count = 3;

    private readonly UsageValue<T>[] _buffers = new UsageValue<T>[buffer_count];

    protected override int bufferCount => buffer_count;

    protected override UsageValue<T>[] buffers => this._buffers;

    public TripleBuffer() : base()
    {
    }
}
