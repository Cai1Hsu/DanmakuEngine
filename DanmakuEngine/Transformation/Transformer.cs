using System.Diagnostics;
using DanmakuEngine.Logging;
using DanmakuEngine.Transfomation.Functions;

namespace DanmakuEngine.Transfomation;

public class Transformer : ITransformable
{
    public double Duration { get; private set; }

    public double CurrentTime { get; private set; }

    public double time
    {
        get
        {
            var t = CurrentTime / Duration;

            if (t > 1 && !AllowOverflow)
                t = 1;

            if (t < -1 && !AllowOverflow)
                t = -1;

            return t;
        }
    }

    public ITransformFunction? Function { get; private set; }

    public bool IsDone => CurrentTime >= Duration;

    public Action<double> OnUpdate = null!;

    public Action OnDone = null!;

    public bool AllowOverflow = false;

    private bool done = false;

    private bool disposed = false;

    public Transformer(double duration, ITransformFunction function, Action<double> onUpdate)
    {
        if (duration < 0)
            throw new ArgumentException("Duration must be positive");

        Duration = duration;
        Function = function;

        OnDone += () => OnUpdate?.Invoke(Function!.Transform(1));

        OnUpdate += onUpdate;
    }

    public void Update(double deltaTime)
    {
        if (disposed)
            return;

        CurrentTime += deltaTime;

        if (Function == null)
            return;

        if (!done && IsDone)
            OnDone?.Invoke();

        if (done)
            return;

        var value = Function.Transform(time);

        OnUpdate?.Invoke(value);
    }

    public void Reset()
    {
        CurrentTime = 0;
    }

    public void Dispose()
    {
        Dispose(true);

        GC.SuppressFinalize(this);
    }

    public void Dispose(bool disposing)
    {
        if (!disposing)
            return;

        disposed = true;
        OnUpdate = null!;
        OnDone = null!;
        Function = null!;
    }

    public TransformSequence Add(params ITransformable[] transformables)
        => new TransformSequence(this).Add(transformables);

    public TransformSequence Add(ITransformable transformable)
        => new TransformSequence(this).Add(transformable);

    public TransformSequence Then()
        => new TransformSequence(this).Then();

    public TransformSequence Delay(double duration)
        => new TransformSequence(this).Delay(duration);

    public TransformSequence DelayUntil(Func<bool> condition)
        => new TransformSequence(this).DelayUntil(condition);

    public TransformSequence Loop(int count)
        => new TransformSequence(this).Loop(count);

    public TransformSequence LoopForever()
        => new TransformSequence(this).LoopForever();

    public double CurrentExtraTime => IsDone ? CurrentTime - Duration
        : throw new InvalidOperationException($"The transform is not done yet, CurrentTime: {CurrentTime}, Duration: {Duration}");

    public double TotalDuration => Duration;

    public bool IsCurrentDone => IsDone;
}
