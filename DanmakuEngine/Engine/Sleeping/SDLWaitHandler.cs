using Silk.NET.SDL;

namespace DanmakuEngine.Engine.Sleeping;

public class SDLWaitHandler : IWaitHandler
{
    private static Sdl SDL = Sdl.GetApi();

    public bool IsHighResolution => true;

    public void Register()
    {
    }

    public void Wait(double milliseconds)
    {
        SDL.Delay((uint)milliseconds);
    }

    public void Wait(TimeSpan timeSpan)
        => Wait(timeSpan.TotalMilliseconds);
}
