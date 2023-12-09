namespace DanmakuEngine.Allocations;

public partial class DoubleBuffer<T> : MultiBuffer<T>
{
    private const int buffer_count = 2;

    private readonly UsageValue<T>[] _buffers = new UsageValue<T>[buffer_count];

    protected override int bufferCount => buffer_count;

    protected override UsageValue<T>[] buffers => this._buffers;

    public DoubleBuffer() : base()
    {
    }
}
