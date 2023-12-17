namespace DanmakuEngine.Movements;

public interface ICanGetSpeed<T>
{
    public T Speed { get; }
}

public interface ICanSetSpeed<T>
{
    public T Speed { set; }
}
