using Silk.NET.Maths;

namespace DanmakuEngine.Movements;

public abstract class FixedAcceleratedMovementBase<T> : MovementBase<T>, ICanGetAcceleration<T>, ICanGetSpeed<T>
{
    public T Acceleration { get; protected set; }

    public T InitialSpeed { get; protected set; }

    public abstract T Speed { get; }

    protected abstract override T CurrentValue();

    public FixedAcceleratedMovementBase(T initialSpeed, T acceleration)
    {
        this.Acceleration = acceleration;
        this.InitialSpeed = initialSpeed;
    }
}

public class FixedAcceleratedMovementF(float initialSpeed, float acceleration)
    : FixedAcceleratedMovementBase<float>(initialSpeed, acceleration)
{
    public override float Speed => InitialSpeed +
              (Acceleration * (float)ElapsedTime);

    protected override float CurrentValue()
        => startValue +
           (InitialSpeed * (float)ElapsedTime) +
           (Acceleration * (float)ElapsedTime * (float)ElapsedTime / 2);
}

public class FixedAcceleratedMovementD(double initialSpeed, double acceleration)
    : FixedAcceleratedMovementBase<double>(initialSpeed, acceleration)
{
    public override double Speed => InitialSpeed +
              (Acceleration * ElapsedTime);

    protected override double CurrentValue()
        => startValue +
           (InitialSpeed * ElapsedTime) +
           (Acceleration * ElapsedTime * ElapsedTime / 2);
}

public class FixedAcceleratedMovementV2F(Vector2D<float> initialSpeed, Vector2D<float> acceleration)
    : FixedAcceleratedMovementBase<Vector2D<float>>(initialSpeed, acceleration)
{
    public override Vector2D<float> Speed => InitialSpeed +
              (Acceleration * (float)ElapsedTime);

    protected override Vector2D<float> CurrentValue()
        => startValue +
           (InitialSpeed * (float)ElapsedTime) +
           (Acceleration * (float)ElapsedTime * (float)ElapsedTime / 2);
}

public class FixedAcceleratedMovementV2D(Vector2D<double> initialSpeed, Vector2D<double> acceleration)
    : FixedAcceleratedMovementBase<Vector2D<double>>(initialSpeed, acceleration)
{
    public override Vector2D<double> Speed => InitialSpeed +
              (Acceleration * ElapsedTime);

    protected override Vector2D<double> CurrentValue()
        => startValue +
           (InitialSpeed * ElapsedTime) +
           (Acceleration * ElapsedTime * ElapsedTime / 2);
}
