using System.Numerics;
using Silk.NET.Maths;

namespace DanmakuEngine.Movements;

public abstract class FixedLinearMovementBase<T> : MovementBase<T>, ICanGetSpeed<T>
{
    public T Speed { get; protected set; }

    protected abstract override T CurrentValue();

    public FixedLinearMovementBase(T speed)
    {
        this.Speed = speed;
    }
}

public class FixedLinearMovementF(float speed)
    : FixedLinearMovementBase<float>(speed)
{
    protected override float CurrentValue()
        => startValue + (Speed * (float)ElapsedTime);
}

public class FixedLinearMovementD(double speed)
    : FixedLinearMovementBase<double>(speed)
{
    protected override double CurrentValue()
        => startValue + (Speed * ElapsedTime);
}

public class FixedLinearMovementV2F(Vector2D<float> speed)
    : FixedLinearMovementBase<Vector2D<float>>(speed)
{
    protected override Vector2D<float> CurrentValue()
        => startValue + (Speed * (float)ElapsedTime);
}

public class FixedLinearMovementV2D(Vector2D<double> speed)
    : FixedLinearMovementBase<Vector2D<double>>(speed)
{
    protected override Vector2D<double> CurrentValue()
        => startValue + (Speed * ElapsedTime);
}
