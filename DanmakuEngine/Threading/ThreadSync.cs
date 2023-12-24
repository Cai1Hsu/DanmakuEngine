namespace DanmakuEngine.Threading;

public static class ThreadSync
{
    public static bool IsMainThread { get; internal set; }
    public static bool IsUpdateThread { get; internal set; }

    public static bool IsRenderThread { get; internal set; }

    // we haven't implement audio thread yet
    // public static bool IsAudioThread { get; internal set; }

    public static void ResetAllForCurrentThread()
    {
        IsMainThread = false;
        IsUpdateThread = false;
        IsRenderThread = false;
        // IsAudioThread = false;
    }

    public static bool NotMainThread() => !IsMainThread;

    public static bool NotUpadteThread() => !IsUpdateThread;

    public static bool NotRenderThread() => !IsRenderThread;

    // public static bool NotAudioThread() => !IsAudioThread;
}
