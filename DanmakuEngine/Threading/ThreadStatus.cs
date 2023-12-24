namespace DanmakuEngine.Threading;

public enum ThreadStatus
{
    /// <summary>
    /// The thread has not been initialized yet.
    /// </summary>
    NotInitialized,
    /// <summary>
    /// The thread has been initialized, but has not started running yet.
    /// Or it is running under single-threaded mode.
    /// </summary>
    // Initialized,
    /// <summary>
    /// The thread is working.
    /// </summary>
    Running,
    /// <summary>
    /// The thread was started, but is currently paused.
    /// </summary>
    Paused,
    /// <summary>
    /// The thread has been stopped, but has not been disposed yet.
    /// </summary>
    Zombie,
}