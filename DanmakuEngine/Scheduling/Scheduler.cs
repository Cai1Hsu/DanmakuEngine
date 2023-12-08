using System.Data;
using System.Diagnostics;
using System.Linq.Expressions;
using DanmakuEngine.Allocations;
using DanmakuEngine.Games;
using DanmakuEngine.Logging;
using DanmakuEngine.Timing;
using Silk.NET.Vulkan;

namespace DanmakuEngine.Scheduling;

public class Scheduler : IUpdatable
{
    private readonly Queue<ScheduledTask> tasks = new();

    private readonly object taskLock = new();

    private readonly LazyValue<Queue<ScheduledTask>> pendingTasks = new(() => new());

    private double CurrentTime => Clock.CurrentTime;

    private readonly LazyValue<IClock> clock;

    protected IClock Clock => clock.Value;

    /// <summary>
    /// Create a scheduler that uses a clock
    /// </summary>
    /// <param name="clock">the clock</param>
    public Scheduler(IClock clock)
    {
        this.clock = new(clock);
    }

    /// <summary>
    /// Create a scheduler that uses a lazy-initialized clock delegate
    /// </summary>
    /// <param name="getClock">the delegate to get a clock</param>
    public Scheduler(Func<IClock> getClock)
    {
        this.clock = new(getClock);
    }

    /// <summary>
    /// Create a scheduler that uses a lazy-initialized clock
    /// </summary>
    /// <param name="clock">the <see cref="LazyValue{Clock}"/> for scheduler</param>
    public Scheduler(LazyValue<IClock> lazyClock)
    {
        this.clock = lazyClock;
    }

    /// <summary>
    /// Create a scheduler that uses a lazy-initialized clock
    /// </summary>
    public Scheduler()
    {
        this.clock = new LazyValue<IClock>(() => new TimeProvider());
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
            while (tasks.Count > 0)
            {
                var task = tasks.Dequeue();

                if (task.ShouldRun)
                {
                    using var t = task;

                    t.Run();
                }
                else
                {
                    pendingTasks.Value.Enqueue(task);
                }
            }

            if (pendingTasks.HasValue && pendingTasks.Value.Count > 0)
            {
                while (pendingTasks.Value.Count > 0)
                    tasks.Enqueue(pendingTasks.Value.Dequeue());
            }
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

    private class TimeProvider : IClock
    {
        public double UpdateDelta => Time.UpdateDelta;

        public double RenderDelta => Time.RenderDelta;

        public double CurrentTime => Time.CurrentTime;
    }
}