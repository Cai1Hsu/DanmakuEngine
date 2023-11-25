namespace DanmakuEngine.Scheduling;

public class ScheduledTask : IDisposable
{
    private Action action;

    private bool IsDisposed = false;

    public virtual bool ShouldRun => true;

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
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (!disposing)
            return;

        this.action = null!;

        IsDisposed = true;
    }
}