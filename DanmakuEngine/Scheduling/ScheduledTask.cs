namespace DanmakuEngine.Scheduling;

public class ScheduledTask : IDisposable
{
    private Action action;

    private bool IsDisposed = false;

    public virtual bool ShouldRun
        => _shouldRun is null || _shouldRun.Invoke();

    private readonly Func<bool>? _shouldRun;

    public ScheduledTask(Action action, Func<bool> shouldRun)
    {
        this.action = action;

        this._shouldRun = shouldRun;
    }

    public ScheduledTask(Action action)
    {
        this.action = action;
    }

    public void Run()
    {
        if (IsDisposed)
            throw new InvalidOperationException("Can NOT run a disposed task!");

        action.Invoke();
    }

    public void Dispose()
    {
        this.action = null!;

        IsDisposed = true;

        GC.SuppressFinalize(this);
    }
}
