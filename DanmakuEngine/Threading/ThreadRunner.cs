// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.

using System.Diagnostics;
using DanmakuEngine.Bindables;
using DanmakuEngine.Engine.Threading;
using DanmakuEngine.Logging;

namespace DanmakuEngine.Threading;

/// <summary>
/// Runs a game host in a specific threading mode.
/// </summary>
public class ThreadRunner
{
    private readonly MainThread _mainThread;

    private readonly List<GameThread> _threads = new();

    public IReadOnlyCollection<GameThread> Threads
    {
        get
        {
            lock (_threads)
                return _threads.ToArray();
        }
    }

    private readonly object startStopLock = new object();

    /// <summary>
    /// Construct a new ThreadRunner instance.
    /// </summary>
    /// <param name="mainThread">The main window thread. Used for input in multi-threaded execution; all game logic in single-threaded execution.</param>
    /// <exception cref="NotImplementedException"></exception>
    public ThreadRunner(MainThread mainThread)
    {
        this._mainThread = mainThread;
        AddThread(mainThread);
    }

    /// <summary>
    /// Add a new non-main thread. In single-threaded execution, threads will be executed in the order they are added.
    /// </summary>
    public void AddThread(GameThread thread)
    {
        lock (_threads)
        {
            if (!_threads.Contains(thread))
                _threads.Add(thread);
        }
    }

    /// <summary>
    /// Remove a non-main thread.
    /// </summary>
    public void RemoveThread(GameThread thread)
    {
        lock (_threads)
            _threads.Remove(thread);
    }


    private bool? runningMultiThreaded = null;
    public readonly Bindable<bool> MultiThreaded = new(true);

    public virtual void RunMainLoop()
    {
        // propagate any requested change in execution mode at a safe point in frame execution
        ensureCorrectThreadingMode();

        Debug.Assert(runningMultiThreaded.HasValue);

        if (runningMultiThreaded.Value)
        {
            // Other threads will run themselves
            _mainThread?.RunSingleFrame();
        }
        else
        {
            lock (_threads)
            {
                foreach (var t in _threads)
                    t.RunSingleFrame();
            }
        }

        ThreadSync.ResetAllForCurrentThread();
    }

    public void Start() => ensureCorrectThreadingMode();

    public void Suspend()
    {
        lock (startStopLock)
        {
            pauseAllThreads();
        }
    }

    public void Stop()
    {
        const int thread_join_timeout = 30000;

        // exit in reverse order so AudioThread is exited last (UpdateThread depends on AudioThread)
        foreach (var t in Threads.Reverse())
        {
            // save the native thread to a local variable as Thread gets set to null when exiting.
            // WaitForState(Exited) appears to be unsafe in multithreaded.
            var thread = t.Thread;

            t.Exit();

            if (thread != null)
            {
                if (!thread.Join(thread_join_timeout))
                    throw new TimeoutException($"Thread {t.Type} failed to exit in allocated time ({thread_join_timeout}ms).");
            }
            else
            {
                t.WaitForState(ThreadStatus.Zombie);
            }

            Debug.Assert(t.State.Value == ThreadStatus.Zombie);
        }

        ThreadSync.ResetAllForCurrentThread();
    }

    private void ensureCorrectThreadingMode()
    {
        // locking is required as this method may be called from two different threads.
        lock (startStopLock)
        {
            // pull into a local variable as the property is not locked during writes.
            var multithreaded = MultiThreaded.Value;

            if (multithreaded == runningMultiThreaded)
                return;

            runningMultiThreaded = ThreadSync.MultiThreaded = multithreaded;
            Logger.Debug($"Threading mode changed to {(runningMultiThreaded.Value ? "Multi-threaded" : "Single-threaded")} mode.");
        }

        pauseAllThreads();

        if (runningMultiThreaded.Value)
        {
            // switch to multi-threaded
            foreach (var t in Threads)
                t.Start();
        }
        else
        {
            // switch to single-threaded.
            foreach (var t in Threads)
            {
                t.Initialize();
            }

            // this is usually done in the execution loop, but required here for the initial game startup,
            // which would otherwise leave values in an incorrect state.
            ThreadSync.ResetAllForCurrentThread();
        }
    }

    private void pauseAllThreads()
    {
        // shut down threads in reverse to ensure audio stops last (other threads may be waiting on a queued event otherwise)
        foreach (var t in Threads.Reverse())
            t.Pause();
    }

    public void WaitUntilAllThreadsExited()
    {
        foreach (var t in Threads)
            t.WaitForState(ThreadStatus.Zombie);
    }
}
