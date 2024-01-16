using Silk.NET.Core.Contexts;
using Silk.NET.SDL;

namespace DanmakuEngine.Engine;

public unsafe class SdlGlContext : INativeContext
{
    private Sdl _sdl;

    public SdlGlContext(Sdl sdl)
    {
        ArgumentNullException.ThrowIfNull(sdl);

        this._sdl = sdl;
    }

    private nint getProcAddress(string symbol)
    {
        const int error_category = (int)LogCategory.Error;

        var oldPriority = _sdl.LogGetPriority(error_category);

        // Prevent logging calls to SDL_GL_GetProcAddress() that fail on systems which don't have the requested symbol (typically macOS).
        _sdl.LogSetPriority(error_category, LogPriority.LogPriorityInfo);

        nint ret = (nint)_sdl.GLGetProcAddress(symbol);

        // Reset the logging behaviour.
        _sdl.LogSetPriority(error_category, oldPriority);

        return ret;
    }

    public nint GetProcAddress(string symbol, int? slot = null)
    {
        return getProcAddress(symbol);
    }

    public bool TryGetProcAddress(string symbol, out nint addr, int? slot = null)
    {
        try
        {
            addr = getProcAddress(symbol);

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
