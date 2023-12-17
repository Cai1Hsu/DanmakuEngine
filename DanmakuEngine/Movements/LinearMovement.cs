using System.Numerics;
using DanmakuEngine.Bindables;
using Silk.NET.Maths;

namespace DanmakuEngine.Movements;

public abstract class LinearMovementBase<T> : AccumulatedMovement<T>, ICanGetSpeed<T>, ICanSetSpeed<T>
    where T : IEquatable<T>
{
    protected double currentPeriodStartTime;

    private T _speed = default!;

    public T Speed
    {
        get => _speed;
        set
        {
            if (_speed.Equals(value))
                return;

            _speed = value;

            currentPeriodStartTime = CurrentTime;
        }
    }

    public LinearMovementBase(T speed)
    {
        this.Speed = speed;

        Active.BindValueChanged(e =>
        {
            if (e.NewValue)
                currentPeriodStartTime = CurrentTime;
        });
    }
}

public class LinearMovementF(float speed)
    : LinearMovementBase<float>(speed)
{
    protected override float AccumulatedValue()
        => Value.Value +
           // we still want to try to avoid accumulating to prevent floating point errors
           Speed * (float)(CurrentTime - currentPeriodStartTime);

}

public class LinearMovementD(double speed)
    : LinearMovementBase<double>(speed)
{
    protected override double AccumulatedValue()
        => Value.Value +
           // we still want to try to avoid accumulating to prevent floating point errors
           Speed * (CurrentTime - currentPeriodStartTime);
}

public class LinearMovementV2F(Vector2D<float> speed)
    : LinearMovementBase<Vector2D<float>>(speed)
{
    protected override Vector2D<float> AccumulatedValue()
        => Value.Value +
           // we still want to try to avoid accumulating to prevent floating point errors
           Speed * (float)(CurrentTime - currentPeriodStartTime);
}

public class LinearMovementV2D(Vector2D<double> speed)
    : LinearMovementBase<Vector2D<double>>(speed)
{
    protected override Vector2D<double> AccumulatedValue()
        => Value.Value +
           // we still want to try to avoid accumulating to prevent floating point errors
           Speed * (CurrentTime - currentPeriodStartTime);
}
