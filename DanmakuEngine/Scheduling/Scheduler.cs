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
            // TODO: Implement this
            // while (tasks.Count > 0 && tasks.Peek().Time <= Clock.Time)
            // {
            //     tasks.Dequeue().Run();
            // }
        }
    }

    public virtual void Update()
    {

    }
}