using System.Diagnostics;
using DanmakuEngine.Logging;
using DanmakuEngine.Transfomation.Functions;

namespace DanmakuEngine.Transfomation;

public class Transformer : ITransformable
{
    public double Duration { get; private set; }

    public double CurrentTime { get; private set; }

    public double time => CurrentTime / Duration;

    public ITransformFunction? Function { get; private set; }

    public bool IsDone => CurrentTime >= Duration;

    public Action<double> OnUpdate = null!;

    public Action OnDone = null!;

    private bool disposed = false;

    public Transformer(double duration, ITransformFunction function, Action<double> onUpdate)
    {
        if (duration < 0)
            throw new ArgumentException("Duration must be positive");

        Duration = duration;
        Function = function;

        OnDone += () => OnUpdate?.Invoke(Function!.Transform(1));
        OnDone += () => OnDone = null!;

        OnUpdate += onUpdate;
    }

    public void Update(double deltaTime)
    {
        if (disposed)
            return;

        CurrentTime += deltaTime;

        if (IsDone)
        {
            OnDone?.Invoke();

            return;
        }

        if (Function == null)
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
        Function = null!;
    }
}
