using DanmakuEngine.Engine.Sleeping;

namespace DanmakuEngine.Engine.Platform.Linux;

public class LinuxWaitHandler : IWaitHandler
{
    public bool IsHighResolution => true;

    public void Register()
    {
        // nothing to do here
    }

    public void Wait(double milliseconds)
    {
        Thread.Sleep((int)milliseconds);
    }

    public void Wait(TimeSpan timeSpan)
    {
        Thread.Sleep(timeSpan);
    }
}