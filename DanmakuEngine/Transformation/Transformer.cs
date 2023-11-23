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

    public Transformer(double duration, ITransformFunction function, bool loop = false)
    {
        if (Duration < 0)
            throw new ArgumentException("Duration must be positive");

        Duration = duration;
        Function = function;

        if (loop)
            return;

        OnDone += () => OnDone = null!;
        OnDone += Dispose;
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

        if (time > 1)
            OnUpdate?.Invoke(Function.Transform(1));
        else
            OnUpdate?.Invoke(Function.Transform(time));
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
