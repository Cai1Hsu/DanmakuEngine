using Silk.NET.Maths;

namespace DanmakuEngine.Movements;

public abstract class FixedAcceleratedMovementBase<T> : MovementBase<T>, ICanGetAcceleration<T>, ICanGetSpeed<T>
{
    public T Acceleration { get; protected set; }

    public T InitialSpeed { get; protected set; }

    public abstract T Speed { get; }

    protected abstract override T CurrentValue();

    public FixedAcceleratedMovementBase(T initialSpeed, T Acceleration)
    {
        this.Acceleration = Acceleration;
        this.InitialSpeed = initialSpeed;
    }
}

public class FixedAcceleratedMovementF(float initialSpeed, float Acceleration)
    : FixedAcceleratedMovementBase<float>(initialSpeed, Acceleration)
{
    public override float Speed => InitialSpeed +
              Acceleration * (float)ElapsedTime;

    protected override float CurrentValue()
        => startValue +
           InitialSpeed * (float)ElapsedTime +
           Acceleration * (float)ElapsedTime * (float)ElapsedTime / 2;
}

public class FixedAcceleratedMovementD(double initialSpeed, double Acceleration)
    : FixedAcceleratedMovementBase<double>(initialSpeed, Acceleration)
{
    public override double Speed => InitialSpeed +
              Acceleration * ElapsedTime;

    protected override double CurrentValue()
        => startValue +
           InitialSpeed * ElapsedTime +
           Acceleration * ElapsedTime * ElapsedTime / 2;
}

public class FixedAcceleratedMovementV2F(Vector2D<float> initialSpeed, Vector2D<float> Acceleration)
    : FixedAcceleratedMovementBase<Vector2D<float>>(initialSpeed, Acceleration)
{
    public override Vector2D<float> Speed => InitialSpeed +
              Acceleration * (float)ElapsedTime;

    protected override Vector2D<float> CurrentValue()
        => startValue +
           InitialSpeed * (float)ElapsedTime +
           Acceleration * (float)ElapsedTime * (float)ElapsedTime / 2;
}

public class FixedAcceleratedMovementV2D(Vector2D<double> initialSpeed, Vector2D<double> Acceleration)
    : FixedAcceleratedMovementBase<Vector2D<double>>(initialSpeed, Acceleration)
{
    public override Vector2D<double> Speed => InitialSpeed +
              Acceleration * ElapsedTime;

    protected override Vector2D<double> CurrentValue()
        => startValue +
           InitialSpeed * ElapsedTime +
           Acceleration * ElapsedTime * ElapsedTime / 2;
}
