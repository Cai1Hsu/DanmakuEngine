using DanmakuEngine.Logging;
using DanmakuEngine.Scheduling;

namespace DanmakuEngine.Threading;

public abstract class GameThread
{
    public Thread? thread { get; private set; }

    public ThrottledExecutor executor { get; private set; }

    public ThreadType type { get; protected set; }

    public ThreadStatus Status { get; protected set; } = ThreadStatus.NotInitialized;

    private ThreadStatus? requestedStatus = null;

    public abstract bool IsCurrent { get; }

    protected virtual void OnInitialize()
    {
    }

    public virtual void Initialize()
    {
        MakeCurrent();

        OnInitialize();

        Status = ThreadStatus.Running;
    }

    public void Start()
    {
        if (Status >= ThreadStatus.Running)
            throw new InvalidOperationException("Thread is already running.");

        Initialize();

        thread?.Start();
    }

    public void Pause()
    {
        if (Status != ThreadStatus.Running)
            return;

        // request pause and wait until the thread is paused
        WaitUntilTargetStatus(ThreadStatus.Paused);
    }

    public void WaitUntilTargetStatus(ThreadStatus targetStatus)
    {
        if (Status == targetStatus)
            return;

        // request change
        requestedStatus = targetStatus;

        // not Main thread
        if (thread is not null)
        {
            while (Status != targetStatus)
                Thread.Sleep(1);
        }
        else
        {
            // Main thread
            while (Status != targetStatus)
                RunNextFrame();
        }
    }

    public void RunNextFrame()
    {
        if (requestedStatus.HasValue)
        {
            Status = requestedStatus.Value;
            requestedStatus = null;
        }

        if (Status is not ThreadStatus.Running)
            return;

        MakeCurrent();

        try
        {
            executor.RunNextFrame();
        }
        catch (Exception e)
        {
            Logger.Error($"[{type}] Unhandled exception: {e.Message}\n{e.StackTrace}");
        }
    }

    public virtual void MakeCurrent()
    {
        ThreadSync.ResetAllForCurrentThread();
    }

    public GameThread(Action<double> runNextFrame, ThreadType type)
    {
        executor = new ThrottledExecutor(runNextFrame);

        this.type = type;

        if (type == ThreadType.Main)
            return;

        thread = new Thread(work)
        {
            IsBackground = true,
            Name = $"GameThread-{type}"
        };
    }

    protected virtual void work()
    {
        Initialize();

        while (true)
            RunNextFrame();
    }
}
