using DanmakuEngine.Bindables;
namespace DanmakuEngine.Threading;

public class ThreadRunner
{
    private bool runningMultiThreaded = true;
    public readonly Bindable<bool> MultiThreaded = new(true);

    private readonly List<GameThread> threads = new();

    public IReadOnlyCollection<GameThread> Threads
    {
        get
        {
            lock (threads)
                return threads.ToArray();
        }
    }

    public void Add(GameThread thread)
        => threads.Add(thread);

    public void Remove(GameThread thread)
        => threads.Remove(thread);

    public void RunMainLoop()
    {
        // Single-threaded mode
        // we have to run the threads manually
        if (!runningMultiThreaded)
        {
            foreach (var t in threads)
                t.RunNextFrame();
        }
        else
        {
            // However in multi-threaded mode, threads will run themselves as long as they are started.
            // we only need to run the main thread.
            foreach (var t in threads)
            {
                if (t.type != ThreadType.Main)
                    continue;

                t.RunNextFrame();
            }
        }

        ThreadSync.ResetAllForCurrentThread();
    }

    public void Start()
    {
        pauseAllThreads();

        resumeAllThreads();
    }

    public void Pause() => pauseAllThreads();

    public void Stop()
    {
        pauseAllThreads();

        foreach (var t in threads)
            t.Stop();
    }

    private void pauseAllThreads()
    {
        // shut down threads in reverse to ensure audio stops last (other threads may be waiting on a queued event otherwise)
        foreach (var t in Threads.Reverse())
            t.Pause();
    }

    private void resumeAllThreads()
    {
        if (runningMultiThreaded)
        {
            // just leave them running background.
            foreach (var t in threads)
                t.Start();
        }
        else
        {
            foreach (var t in threads)
                t.Initialize();

            // this is usually done in the execution loop, but required here for the initial game startup,
            // which would otherwise leave values in an incorrect state.
            ThreadSync.ResetAllForCurrentThread();
        }
    }

    public ThreadRunner()
    {
        MultiThreaded.BindValueChanged(v =>
        {
            lock (MultiThreaded)
            {
                // will this happen? 
                // but i leave it here just in case
                if (v.NewValue == runningMultiThreaded)
                    return;

                runningMultiThreaded = MultiThreaded.Value;
            }

            pauseAllThreads();

            resumeAllThreads();
        }, true);

        runningMultiThreaded = MultiThreaded.Value;
    }
}

public enum ThreadType
{
    Main,
    Update,
    Render,

    // we have'nt implemented these yet
    // Audio,
}
