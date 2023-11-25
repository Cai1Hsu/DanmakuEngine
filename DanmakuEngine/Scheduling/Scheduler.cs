using DanmakuEngine.Timing;

namespace DanmakuEngine.Scheduling;

public class Scheduler
{
    private readonly Queue<ScheduledTask> tasks = new();
    private readonly object taskLock = new();

    private Clock Clock = new Clock();

    public void ScheduleTask(ScheduledTask task)
    {
        lock (taskLock)
        {
            tasks.Enqueue(task);
        }
    }

    public void update()
    {
        Clock.Update(Time.UpdateDelta);

        lock (taskLock)
        {
            while (tasks.Count > 0)
            {
                var task = tasks.Dequeue();

                if (task.ShouldRun)
                    task.Run();
            }
        }

        Update();
    }

    public virtual void Update()
    {

    }
}