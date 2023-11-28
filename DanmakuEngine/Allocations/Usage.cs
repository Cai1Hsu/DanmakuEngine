namespace DanmakuEngine.Allocations;

public class Usage<T> : IDisposable
{
    public readonly int Index;

    public UsingType UsingType = UsingType.Avaliable;

    private Action<Usage<T>> OnFinish;

    public Usage(int index, Action<Usage<T>> onFinish = null!)
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