using DanmakuEngine.Bindables;
using DanmakuEngine.Transformation.Functions;

namespace DanmakuEngine.Transformation;

public class Transformer : ITransformable
{
    public double Duration { get; private set; }

    public double CurrentTime { get; private set; }

    public double time
        => Math.Clamp(CurrentTime / Duration, -1, 1);

    public ITransformFunction? Function { get; private set; }

    public bool IsDone => CurrentTime >= Duration;

    public Action<double> OnUpdate = null!;

    public double Value => _binding.Value;

    private Bindable<double> _binding = new();

    public Action OnDone = null!;

    private bool done = false;

    private bool disposed = false;

    public Transformer(double duration, ITransformFunction function, Action<double> onUpdate)
    {
        if (duration < 0)
            throw new ArgumentException("Duration must be positive");

        Duration = duration;
        Function = function;

        OnDone += () => OnUpdate?.Invoke(Function!.Transform(1));

        if (onUpdate is null)
            return;

        OnUpdate += onUpdate;
    }

    public Transformer(double duration, ITransformFunction function)
        : this(duration, function, null!)
    {
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

        _binding.Value = value;

        OnUpdate?.Invoke(value);
    }

    public void Reset()
    {
        CurrentTime = 0;
    }

    public Transformer BindTo(Bindable<double> bindable)
    {
        _binding.BindTo(bindable);

        return this;
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
