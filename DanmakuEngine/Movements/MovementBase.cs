using DanmakuEngine.Bindables;
using DanmakuEngine.Games;
using DanmakuEngine.Timing;

namespace DanmakuEngine.Movements;

public abstract class MovementBase<T> : UpdateOnlyObject, IDisposable
{
    public readonly Bindable<T> Value = new();

    public readonly Bindable<bool> Active = new(false);

    public double StartTime { get; private set; }

    public double CurrentTime => _clock.CurrentTime;

    public double ElapsedTime => CurrentTime - StartTime;

    public Action<MovementBase<T>>? PostUpdate = null;

    public Action<MovementBase<T>>? OnDone = null;

    public Func<MovementBase<T>, bool> Condition = _ => true;

    protected T startValue;

    private IClock _clock = Time.Clock;

    protected IClock Clock => _clock;

    public MovementBase<T> SetClock(IClock clock)
    {
        if (_clock != clock)
        {
            _clock = clock;

            StartTime = _clock.CurrentTime;
            startValue = Value.Value;
        }

        return this;
    }

    protected abstract T CurrentValue();

    protected sealed override void Update()
    {
        if (!Active.Value)
            return;

        Active.Value &= Condition(this);

        if (!Active.Value)
        {
            OnDone?.Invoke(this);

            Dispose();

            return;
        }

        Value.Value = CurrentValue();

        PostUpdate?.Invoke(this);
    }

    public MovementBase<T> BindTo(Bindable<T> bindable)
    {
        Value.BindTo(bindable);

        return this;
    }

    public void BeginMove()
    {
        if (Active.Value)
            return;

        StartTime = _clock.CurrentTime;

        startValue = Value.Value;

        Active.Value = true;
    }

    public void Terminate()
    {
        Active.Value = false;

        OnDone?.Invoke(this);

        Dispose();
    }

    public MovementBase()
    {
        startValue = Value.Value;
    }

    public MovementBase(bool begin)
        : this()
    {
        if (begin)
            BeginMove();
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
        {
            OnDone = null!;
            PostUpdate = null!;
        }
    }
}
