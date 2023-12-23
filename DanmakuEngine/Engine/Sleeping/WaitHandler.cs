
namespace DanmakuEngine.Engine.Sleeping;

public static class WaitHandler
{
    public static bool IsHighResolution => handler.IsHighResolution;

    private static IWaitHandler handler => IWaitHandler.WaitHandler;

    public static void Wait(double milliseconds)
        => handler.Wait(milliseconds);

    public static void Wait(TimeSpan timeSpan)
        => handler.Wait(timeSpan);
}