using System.Data;
using System.Diagnostics;
using System.Linq.Expressions;
using DanmakuEngine.Games;
using DanmakuEngine.Logging;
using DanmakuEngine.Timing;
using Silk.NET.Vulkan;

namespace DanmakuEngine.Scheduling;

public class Scheduler : IUpdatable
{
    private readonly Queue<ScheduledTask> tasks = new();
    private readonly object taskLock = new();

    private double CurrentTime => Clock.CurrentTime;

    private Clock? clock;

    protected Clock Clock => clock ??= getClock();

    private readonly Func<Clock> getClock = null!;

    public Scheduler(Clock clock)
    {
        this.clock = clock;
    }

    /// <summary>
    /// Create a scheduler that uses a lazy-initialized clock
    /// </summary>
    /// <param name="getClock"></param>
    public Scheduler(Func<Clock> getClock)
    {
        this.getClock = getClock;

        Debug.Assert(clock == null);
    }

    public void ScheduleTask(ScheduledTask task)
    {
        lock (taskLock)
        {
            tasks.Enqueue(task);
        }
    }

    public void ScheduleTask(Action action, Func<bool> shouldRun)
        => ScheduleTask(new ScheduledTask(action, shouldRun));

    public void ScheduleTask(Action action)
        => ScheduleTask(new ScheduledTask(action));

    /// <summary>
    /// Schedules a task to run after a delay.
    /// </summary>
    /// <param name="task">The task to schedule</param>
    /// <param name="delay">delay in seconds</param>
    public void ScheduleTaskDelay(Action action, double delay)
    {
        if (delay <= 0)
        {
            ScheduleTask(action);
            return;
        }

        var untilTime = CurrentTime + delay;

        ScheduleTask(
            scheduleDelayedTask,
            isDelayFinished
        );

        void scheduleDelayedTask()
            => ScheduleTask(action);

        bool isDelayFinished()
        {
            if (delay <= 0)
                return true;

            return CurrentTime >= untilTime;
        }
    }

    public void load()
        => Load();

    public virtual void Load()
    {
    }

    public void start()
        => Start();

    public virtual void Start()
    {
    }

    public void update()
    {
        lock (taskLock)
        {
            var next = new Queue<ScheduledTask>();

            while (tasks.Count > 0)
            {
                var task = tasks.Dequeue();

                if (task.ShouldRun)
                {
                    using var t = task;

                    task.Run();
                }
                else
                {
                    next.Enqueue(task);
                }
            }

            while (next.Count > 0)
                tasks.Enqueue(next.Dequeue());
        }

        Update();
    }

    /// <summary>
    /// Please do not call this methods directly
    /// you should override <see cref="Update"/> and call <see cref="update"/> instead
    /// </summary>
    public virtual void Update()
    {

    }
}