using System.Data;
using System.Diagnostics;
using System.Linq.Expressions;
using DanmakuEngine.Allocations;
using DanmakuEngine.Games;
using DanmakuEngine.Logging;
using DanmakuEngine.Timing;

namespace DanmakuEngine.Scheduling;

public class Scheduler : UpdateOnlyObject
{
    private readonly Queue<ScheduledTask> tasks = new();

    private readonly object taskLock = new();

    private readonly LazyValue<Queue<ScheduledTask>> pendingTasks = LazyValue.Create<Queue<ScheduledTask>>();

    private double currentTime => Clock.ElapsedSeconds;

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

    public static Scheduler Create<T>(LazyValue<T> lazyClock)
        where T : class, IClock
        => new(lazyClock.ToBase<IClock, T>());

    /// <summary>
    /// Create a scheduler that uses a lazy-initialized clock
    /// </summary>
    public Scheduler()
    {
        this.clock = new LazyValue<IClock>(() => new StandardClock());
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

        var untilTime = currentTime + delay;

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

            return currentTime >= untilTime;
        }
    }

    public bool Empty
    {
        get
        {
            lock (taskLock)
            {
                return tasks.Count == 0;
            }
        }
    }

    private ScheduledTask dequeueTask()
    {
        lock (taskLock)
        {
            if (tasks.Count == 0)
                return null!;

            return tasks.Dequeue();
        }
    }

    protected override void Update()
    {
        while (!Empty)
        {
            var task = dequeueTask();

            if (task is null)
                continue;

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

        lock (taskLock)
        {
            while (pendingTasks.Value.Count > 0)
            {
                tasks.Enqueue(pendingTasks.Value.Dequeue());
            }
        }
    }

    /// <summary>
    /// Change the clock used by this scheduler
    ///
    /// Please note that change the clock of a scheduler that is already in use may cause unexpected behavior
    /// If you are really intended to change the clock when using a scheduler
    /// please make sure the CurrentTime of the new clock is the consistent with the old one
    /// </summary>
    /// <param name="clock">the new clock</param>
    public void ChangeClock(IClock clock)
    {
        if (!Empty)
            throw new InvalidOperationException("Can not change clock when tasks are not empty.");

        this.clock.AssignValue(clock, true);
    }
}
