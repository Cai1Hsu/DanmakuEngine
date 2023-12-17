namespace DanmakuEngine.Movements;

public interface ICanSetAcceleration<T>
{
    public T Acceleration { set; }
}

public interface ICanGetAcceleration<T>
{
    public T Acceleration { get; }
}
