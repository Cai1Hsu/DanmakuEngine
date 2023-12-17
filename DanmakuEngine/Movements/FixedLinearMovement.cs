using System.Numerics;
using Silk.NET.Maths;

namespace DanmakuEngine.Movements;

public abstract class FixedLinearMovementBase<T> : MovementBase<T>, ICanGetSpeed<T>
{
    public T Speed { get; protected set; }

    protected abstract override T CurrentValue();

    public FixedLinearMovementBase(T Speed)
    {
        this.Speed = Speed;
    }
}

public class FixedLinearMovementF(float Speed)
    : FixedLinearMovementBase<float>(Speed)
{
    protected override float CurrentValue()
        => startValue + Speed * (float)ElapsedTime;
}

public class FixedLinearMovementD(double Speed)
    : FixedLinearMovementBase<double>(Speed)
{
    protected override double CurrentValue()
        => startValue + Speed * ElapsedTime;
}

public class FixedLinearMovementV2F(Vector2D<float> Speed)
    : FixedLinearMovementBase<Vector2D<float>>(Speed)
{
    protected override Vector2D<float> CurrentValue()
        => startValue + Speed * (float)ElapsedTime;
}

public class FixedLinearMovementV2D(Vector2D<double> Speed)
    : FixedLinearMovementBase<Vector2D<double>>(Speed)
{
    protected override Vector2D<double> CurrentValue()
        => startValue + Speed * ElapsedTime;
}
