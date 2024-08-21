namespace DanmakuEngine.Threading;

public enum ThreadStatus : byte
{
    Undefined = 0,
    /// <summary>
    /// The thread has not been initialized yet.
    /// </summary>
    NotInitialized = 1,
    /// <summary>
    /// The thread has been initialized, but has not started running yet.
    /// Or it is running under single-threaded mode.
    /// </summary>
    // Initialized,

    /// <summary>
    /// The thread is working.
    /// </summary>
    Running = 1 << 1,

    /// <summary>
    /// The thread was started, but is currently paused.
    /// </summary>
    Paused = 1 << 2,

    /// <summary>
    /// The thread has been stopped, but has not been disposed yet.
    /// </summary>
    Zombie = 1 << ((sizeof(ThreadStatus) * 8) - 1),
}
