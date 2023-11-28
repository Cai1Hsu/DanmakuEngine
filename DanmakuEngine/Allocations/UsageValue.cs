namespace DanmakuEngine.Allocations;

public class UsageValue<T> : IDisposable
{
    public T? Value;

    public readonly int Index;

    public UsingType UsingType = UsingType.Avaliable;

    private Action<UsageValue<T>> OnFinish;

    public UsageValue(int index, Action<UsageValue<T>> onFinish = null!)
    {
        this.Index = index;
        this.OnFinish = onFinish;
    }

    public void Dispose()
    {
        OnFinish?.Invoke(this);
    }
}

public enum UsingType
{
    Avaliable,
    Writing,
    Reading,
}