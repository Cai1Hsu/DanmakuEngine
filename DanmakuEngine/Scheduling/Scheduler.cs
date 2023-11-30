namespace DanmakuEngine.Scheduling;

public class Scheduler
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

                    task.Run();
                }
            }
        }

        Update();
    }

    public virtual void Update()
    {

    }
}