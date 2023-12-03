using DanmakuEngine.Games;

namespace DanmakuEngine.Scheduling;

public class Scheduler : IUpdatable
{
    private readonly Queue<ScheduledTask> tasks = new();
    private readonly object taskLock = new();

    public void ScheduleTask(ScheduledTask task)
    {
        lock (taskLock)
        {
            tasks.Enqueue(task);
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

    public virtual void Update()
    {

    }
}