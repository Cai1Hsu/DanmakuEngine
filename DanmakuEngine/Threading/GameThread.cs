// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.

using System.Diagnostics;
using DanmakuEngine.Bindables;
using DanmakuEngine.Logging;
using DanmakuEngine.Scheduling;

namespace DanmakuEngine.Threading;

/// <summary>
/// A conceptual thread used for running game work. May or may not be backed by a native thread.
/// </summary>
public class GameThread
{
    /// <summary>
    /// Whether the game is active (in the foreground).
    /// </summary>
    public readonly Bindable<bool> IsActive = new(true);

    public readonly Bindable<double> ActiveHz = new(1000);
    public readonly Bindable<double> BackgroundHz = new(1000);

    public double ElapsedSeconds { get; private set; }

    public double DeltaTime { get; private set; }

    /// <summary>
    /// Frame time jitter in milliseconds
    /// </summary>
    public double Jitter { get; private set; }

    public double AverageFramerate => Executor.AverageFramerate;

    /// <summary>
    /// The current state of this thread.
    /// </summary>
    internal Bindable<ThreadStatus> State => state;

    private readonly Bindable<ThreadStatus> state = new(ThreadStatus.NotInitialized);

    /// <summary>
    /// Whether this thread is currently running.
    /// </summary>
    public bool Running => state.Value == ThreadStatus.Running;

    /// <summary>
    /// Whether this thread is exited.
    /// </summary>
    public bool Exited => state.Value == ThreadStatus.Zombie;

    /// <summary>
    /// Whether currently executing on this thread (from the point of invocation).
    /// </summary>
    public virtual bool IsCurrent => true;

    /// <summary>
    /// The current dedicated OS thread for this <see cref="GameThread"/>.
    /// A value of <see langword="null"/> does not necessarily mean that this thread is not running;
    /// in <see cref="ThreadingMode.SingleThread"/> threading mode <see cref="ThreadRunner"/> drives its <see cref="GameThread"/>s
    /// manually and sequentially on the main OS thread of the game process.
    /// </summary>
    internal Thread? Thread { get; private set; }

    /// <summary>
    /// A synchronisation context which posts to this thread.
    /// </summary>
    public SynchronizationContext SynchronizationContext => synchronizationContext;

    private readonly SynchronizationContext synchronizationContext;

    private readonly ManualResetEvent initializedEvent = new ManualResetEvent(false);

    private readonly object startStopLock = new object();

    /// <summary>
    /// Whether a pause has been requested.
    /// </summary>
    private volatile bool pauseRequested;

    /// <summary>
    /// Whether an exit has been requested.
    /// </summary>
    private volatile bool exitRequested;

    public readonly ThrottledExecutor Executor;

    public readonly ThreadType Type;

    internal GameThread(Action onNewFrame, ThreadType type)
        : this(onNewFrame, type, new ThrottledExecutor(onNewFrame))
    {
    }

    internal GameThread(Action onNewFrame, ThreadType type,
        ThrottledExecutor executor)
    {
        Executor = executor;

        Type = type;

        synchronizationContext = new SynchronizationContext();

        ActiveHz.BindValueChanged(v =>
        {
            RequestFrequencyChange(activeHz: v.NewValue);
        }, true);

        BackgroundHz.BindValueChanged(v =>
        {
            RequestFrequencyChange(inactiveHz: v.NewValue);
        }, true);
    }

    /// <summary>
    /// Block until this thread has entered an initialized state.
    /// </summary>
    public void WaitUntilInitialized()
    {
        initializedEvent.WaitOne();
    }

    /// <summary>
    /// Returns a string representation that is suffixed with a game thread identifier.
    /// </summary>
    /// <param name="name">The content to suffix.</param>
    /// <returns>A suffixed string.</returns>
    public static string SuffixedThreadNameFor(string name) => $"{name} ({nameof(GameThread)})";

    /// <summary>
    /// Start this thread.
    /// </summary>
    /// <remarks>
    /// This method blocks until in a running state.
    /// </remarks>
    public void Start()
    {
        lock (startStopLock)
        {
            switch (state.Value)
            {
                case ThreadStatus.Paused:
                case ThreadStatus.NotInitialized:
                    break;

                default:
                    throw new InvalidOperationException($"Cannot start when thread is {state.Value}.");
            }

            Debug.Assert(state.Value is ThreadStatus.NotInitialized);

            PrepareForWork();
        }

        WaitForState(ThreadStatus.Running);
        Debug.Assert(state.Value == ThreadStatus.Running);
    }

    /// <summary>
    /// Request that this thread is exited.
    /// </summary>
    /// <remarks>
    /// This does not block and will only queue an exit request, which is processed in the main frame loop.
    /// </remarks>
    /// <exception cref="InvalidOperationException">Thrown when attempting to exit from an invalid state.</exception>
    public void Exit()
    {
        lock (startStopLock)
        {
            switch (state.Value)
            {
                // technically we could support this, but we don't use this yet and it will add more complexity.
                case ThreadStatus.Paused:
                case ThreadStatus.NotInitialized:
                // What the fuck?
                case ThreadStatus.Undefined:
                    throw new InvalidOperationException($"Cannot exit when thread is {state.Value}.");

                case ThreadStatus.Zombie:
                    return;

                default:
                    // actual exit will be done in ProcessFrame.
                    exitRequested = true;
                    break;
            }
        }
    }

    /// <summary>
    /// Prepare this thread for performing work. Must be called when entering a running state.
    /// </summary>
    /// <param name="withThrottling">Whether this thread's clock should be throttling via thread sleeps.</param>
    internal void Initialize()
    {
        lock (startStopLock)
        {
            Debug.Assert(state.Value != ThreadStatus.Running);
            Debug.Assert(state.Value != ThreadStatus.Zombie);

            MakeCurrent();

            OnInitialize();

            initializedEvent.Set();
            state.Value = ThreadStatus.Running;
        }
    }

    /// <summary>
    /// Run when thread transitions into an active/processing state, at the beginning of each frame.
    /// </summary>
    internal virtual void MakeCurrent()
    {
        ThreadSync.ResetAllForCurrentThread();

        SynchronizationContext.SetSynchronizationContext(synchronizationContext);
    }

    /// <summary>
    /// Runs a single frame, updating the execution state if required.
    /// </summary>
    internal void RunSingleFrame()
    {
        var newState = processFrame();

        if (newState.HasValue)
            setExitState(newState.Value);
    }

    /// <summary>
    /// Pause this thread. Must be run from <see cref="ThreadRunner"/> in a safe manner.
    /// </summary>
    /// <remarks>
    /// This method blocks until in a paused state.
    /// </remarks>
    internal void Pause()
    {
        lock (startStopLock)
        {
            if (state.Value != ThreadStatus.Running)
                return;

            // actual pause will be done in ProcessFrame.
            pauseRequested = true;
        }

        WaitForState(ThreadStatus.Paused);
    }

    /// <summary>
    /// Spin indefinitely until this thread enters a required state.
    /// For cases where no native thread is present, this will run <see cref="processFrame"/> until the required state is reached.
    /// </summary>
    /// <param name="targetState">The state to wait for.</param>
    internal void WaitForState(ThreadStatus targetState)
    {
        if (state.Value == targetState)
            return;

        if (Thread == null)
        {
            ThreadStatus? newState = null;

            // if the thread is null at this point, we need to assume that this WaitForState call is running on the same native thread as this GameThread has/will be running.
            // run frames until the required state is reached.
            while (newState != targetState)
                newState = processFrame();

            // note that the only state transition here can be an exiting one. entering a running state can only occur in Initialize().
            setExitState(newState.Value);
        }
        else
        {
            while (state.Value != targetState)
                Thread.Sleep(1);
        }

        Debug.Assert(state.Value == targetState);
    }

    /// <summary>
    /// Prepares this game thread for work. Should block until <see cref="Initialize"/> has been run.
    /// </summary>
    protected virtual void PrepareForWork()
    {
        Debug.Assert(Thread == null);
        createThread();
        Debug.Assert(Thread != null);

        Thread.Start();
    }

    /// <summary>
    /// Called whenever the thread is initialized. Should prepare the thread for performing work.
    /// </summary>
    protected virtual void OnInitialize()
    {
    }

    /// <summary>
    /// Called when a <see cref="Pause"/> or <see cref="Exit"/> is requested on this <see cref="GameThread"/>.
    /// Use this method to release exclusive resources that the thread could have been holding in its current execution mode,
    /// like GL contexts or similar.
    /// </summary>
    protected virtual void OnSuspended()
    {
    }

    /// <summary>
    /// Called when the thread is exited. Should clean up any thread-specific resources.
    /// </summary>
    protected virtual void OnExit()
    {
    }

    /// <summary>
    /// Create the native backing thread to run work.
    /// </summary>
    /// <remarks>
    /// This does not start the thread, but guarantees <see cref="Thread"/> is non-null.
    /// </remarks>
    private void createThread()
    {
        Debug.Assert(Thread == null);
        Debug.Assert(!Running);

        Thread = new Thread(runWork)
        {
            Name = $"GameThread-{Type}",
            IsBackground = true,
        };

        void runWork()
        {
            Initialize();

            while (Running)
                RunSingleFrame();
        }
    }

    /// <summary>
    /// Process a single frame of this thread's work.
    /// </summary>
    /// <returns>A potential execution state change.</returns>
    private ThreadStatus? processFrame()
    {
        if (state.Value != ThreadStatus.Running)
            // host could be in a suspended state. the input thread will still make calls to ProcessFrame so we can't throw.
            return null;

        MakeCurrent();

        if (exitRequested)
        {
            exitRequested = false;
            return ThreadStatus.Zombie;
        }

        if (pauseRequested)
        {
            pauseRequested = false;
            return ThreadStatus.Paused;
        }

        try
        {
            if (requestedActiveHz.HasValue)
            {
                Executor.ActiveHz = requestedActiveHz.Value;
                requestedActiveHz = null;
            }

            if (requestedInactiveHz.HasValue)
            {
                Executor.BackgroundHz = requestedInactiveHz.Value;
                requestedInactiveHz = null;
            }

            Debug.Assert(Executor.ActiveHz == ActiveHz.Value);
            Debug.Assert(Executor.BackgroundHz == BackgroundHz.Value);

            Executor.RunFrame();

            postRunFrame();
        }
        catch (Exception e)
        {
            Logger.Error($"[Thread-{Type}] Unhandled exception: {e.Message}\n{e.StackTrace}");

            var handlerList = OnException?.GetInvocationList();

            if (handlerList != null)
            {
                foreach (var handler in handlerList)
                {
                    if (handler is not Func<Exception, bool> exceptionHandler)
                        continue;

                    if (exceptionHandler(e))
                    {
                        Logger.Debug($"[Thread-{Type}] Exception handled by subscriber.");
                        return null;
                    }
                }
            }

            throw;
        }

        return null;
    }

    protected virtual void postRunFrame()
    {
        ElapsedSeconds = Executor.CurrentTime;
        DeltaTime = Executor.DeltaTime;
        Jitter = Executor.Jitter * 1000;
    }

    private void setExitState(ThreadStatus exitState)
    {
        lock (startStopLock)
        {
            Debug.Assert(state.Value == ThreadStatus.Running);
            Debug.Assert(exitState == ThreadStatus.Zombie || exitState == ThreadStatus.Paused);

            Thread = null;
            OnSuspended();

            if (exitState is ThreadStatus.Zombie)
            {
                initializedEvent?.Dispose();

                OnExit();
            }

            state.Value = exitState;
        }
    }

    public event Func<Exception, bool>? OnException;

    private double? requestedActiveHz = null!;
    private double? requestedInactiveHz = null!;
    public void RequestFrequencyChange(double? activeHz = null, double? inactiveHz = null)
    {
        if (activeHz.HasValue)
            requestedActiveHz = activeHz;

        if (inactiveHz.HasValue)
            requestedInactiveHz = inactiveHz;
    }
}
