using Silk.NET.Core.Contexts;
using Silk.NET.SDL;

namespace DanmakuEngine.Engine;

public unsafe class SdlGlContext : INativeContext
{
    private Sdl sdl;

    public SdlGlContext(Sdl sdl)
    {
        if (sdl== null)
            throw new ArgumentNullException(nameof(sdl));

        this.sdl = sdl;
    }

    public nint GetProcAddress(string s, int? slot = null)
    {
        return (nint)sdl.GLGetProcAddress(s);
    }

    public bool TryGetProcAddress(string s, out nint addr, int? slot = null)
    {
        try
        {
            addr = (nint)sdl.GLGetProcAddress(s);

            return true;
        }
        catch (Exception)
        {
            addr = 0;
        }
        return false;
    }

    public void Dispose()
    {
    }
}