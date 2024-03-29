using System.Runtime.CompilerServices;
using Silk.NET.Core.Contexts;
using Silk.NET.Maths;
using Silk.NET.SDL;

namespace DanmakuEngine.Engine;

/// <summary>
/// Represents an OpenGL context binded to an SDL window.
/// </summary>
public sealed unsafe class SdlGlWindowContext : IGLContext, IDisposable
{
    private Sdl _sdl;

    private void* _context;

    private Window* _window;

    private IGLContextSource _source = null!;

    public nint Handle => (nint)_context;

    public IGLContextSource? Source => _source;

    public bool IsCurrent => _sdl.GLGetCurrentContext() == _context;

    public bool VSync
    {
        get => _sdl.GLGetSwapInterval() == 0;
        set => _sdl.GLSetSwapInterval(value ? 1 : 0);
    }

    public Vector2D<int> FramebufferSize
    {
        get
        {
            var ret = stackalloc int[2];
            _sdl.GLGetDrawableSize(_window, ret, &ret[1]);
            _sdl.ThrowError();
            return *(Vector2D<int>*)ret;
        }
    }

    public SdlGlWindowContext(Sdl sdl, Window* window, IGLContextSource source)
    {
        ArgumentNullException.ThrowIfNull(_sdl = sdl);

        ArgumentNullException.ThrowIfNull(_window = window);

        _context = _sdl.GLCreateContext(window);

        if (_context == null)
            throw new Exception("Failed to create OpenGL context.");
    }

    public nint GetProcAddress(string symbol)
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
        => GetProcAddress(symbol);

    public bool TryGetProcAddress(string symbol, out nint addr, int? slot = null)
    {
        try
        {
            _sdl.ClearError();
            addr = GetProcAddress(symbol);

            if (!string.IsNullOrWhiteSpace(_sdl.GetErrorS()))
            {
                _sdl.ClearError();
                return false;
            }

            return addr != 0;
        }
        catch (Exception)
        {
            addr = 0;
        }
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SwapInterval(int interval)
        => _sdl.GLSetSwapInterval(interval);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SwapBuffers()
        => _sdl.GLSwapWindow(_window);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void MakeCurrent()
        => _sdl.GLMakeCurrent(_window, _context);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IntPtr GetCurrentContext()
        => (IntPtr)_sdl.GLGetCurrentContext();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear()
        => _sdl.GLMakeCurrent(_window, null);

    public void Dispose()
    {
        if (_context != null)
        {
            _sdl.GLDeleteContext(_context);
            _context = null;
        }
    }
}
