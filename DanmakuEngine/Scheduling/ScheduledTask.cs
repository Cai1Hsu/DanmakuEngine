namespace DanmakuEngine.Scheduling;

public class ScheduledTask : IDisposable
{
    private Action action;

    private bool IsDisposed = false;

    public bool ShouldRun => throw new NotImplementedException("sad");

    public ScheduledTask(Action action)
    {
        this.action = action;
    }

    public void Run()
        => action.Invoke();

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

    public void update()
    {

    }
}