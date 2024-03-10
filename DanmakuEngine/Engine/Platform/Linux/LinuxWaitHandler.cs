using System.Runtime.CompilerServices;
using DanmakuEngine.Engine.Sleeping;

namespace DanmakuEngine.Engine.Platform.Linux;

public class LinuxWaitHandler : IWaitHandler
{
    public bool IsHighResolution => true;

    public void Register()
    {
        // nothing to do here
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Wait(double milliseconds)
        => Thread.Sleep((int)milliseconds);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Wait(TimeSpan timeSpan)
        => Thread.Sleep(timeSpan);
}
