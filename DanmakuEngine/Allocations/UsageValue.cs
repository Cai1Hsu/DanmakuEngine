using DanmakuEngine.Scheduling;

namespace DanmakuEngine.Allocations;

public class UsageValue<T> : IDisposable
{
    public T? Value;

    public readonly int Index;

    public UsingType UsingType = UsingType.Avaliable;

    private Action<UsageValue<T>> OnFinish;

    private Scheduler _scheduler = new();
    public Scheduler Scheduler => _scheduler;

    public UsageValue(int index, Action<UsageValue<T>> onFinish = null!)
    {
        this.Index = index;
        this.OnFinish = onFinish;
    }

    public void Dispose()
    {
        OnFinish?.Invoke(this);
        // Execute scheduled tasks
        _scheduler.UpdateSubTree();
    }
}

public enum UsingType
{
    Avaliable,
    Writing,
    Reading,
}
