using System.Runtime.CompilerServices;
using DanmakuEngine.Engine;
using Silk.NET.SDL;

namespace DanmakuEngine.Engine.Sleeping;

public class SDLWaitHandler : IWaitHandler
{
    public bool IsHighResolution => true;

    public void Register()
    {
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Wait(double milliseconds)
        => SDL.Api.Delay((uint)milliseconds);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Wait(TimeSpan timeSpan)
        => Wait(timeSpan.TotalMilliseconds);
}
