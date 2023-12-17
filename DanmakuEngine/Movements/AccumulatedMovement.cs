namespace DanmakuEngine.Movements;

public abstract class AccumulatedMovement<T> : MovementBase<T>
{
    protected abstract T AccumulatedValue();

    protected sealed override T CurrentValue()
        => AccumulatedValue();
}
