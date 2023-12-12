using DanmakuEngine.Bindables;
using DanmakuEngine.Timing;

namespace DanmakuEngine.Movement;

/// <summary>
/// Represents a movement that continues increasing its value until it reaches a certain point
/// </summary>
public class DoubleMovement : IDisposable
{
    /// <summary>
    /// Called after value is updated
    /// use this to update some property
    /// </summary>
    public Action? UpdateValue = null!;

    /// <summary>
    /// do not use this directly, use <see cref="Playing"/>
    /// </summary>
    private bool playing = false;

    private bool done = false;

    /// <summary>
    /// The clock that this movement is based on
    /// </summary>
    protected IClock Clock;

    /// <summary>
    /// The movement stops when the condition returns false
    /// </summary>
    public Func<DoubleMovement, bool> Condition = (_) => true;

    /// <summary>
    /// Called when the movement is done
    /// </summary>
    public Action? OnDone = null!;

    public readonly Bindable<double> Value = new(0);

    public readonly Bindable<double> Speed = new(0);

    private bool disposed = false;

    public bool Done => done || disposed;

    public bool Playing => playing && !done && !Clock.IsPaused;

    public void BeginMove()
    {
        if (done)
            return;

        playing = true;
    }

    public void PauseMove()
        => playing = false;

    public void Stop()
    {
        done = true;
        playing = false;

        Condition = null!;
    }

    public virtual void load()
        => Load();

    public virtual void start()
        => Start();

    public virtual void update()
    {
        if (!Playing)
            return;

        Update();

        UpdateValue?.Invoke();

        if (done |= !Condition(this))
            OnDone?.Invoke();
    }

    /// <summary>
    /// Do we need this?
    /// </summary>
    public virtual void Load()
    {
    }

    /// <summary>
    /// Do we need this?
    /// </summary>
    public virtual void Start()
    {
    }

    /// <summary>
    /// You can override this method to update some property before or after the value is actually updated
    /// </summary>
    public virtual void Update()
        => Value.Value += Speed.Value * Clock.UpdateDelta;

    public DoubleMovement(IClock clock)
    {
        this.Clock = clock;

        OnDone += Dispose;
    }

    public void BindTo(Bindable<double> bindable)
        => Value.BindTo(bindable);

    public void Dispose()
    {
        Condition = null!;
        OnDone = null!;
        UpdateValue = null!;

        disposed = true;

        GC.SuppressFinalize(this);
    }
}
