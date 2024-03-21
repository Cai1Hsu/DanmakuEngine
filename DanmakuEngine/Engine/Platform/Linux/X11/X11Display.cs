using System.Runtime.InteropServices;
using DanmakuEngine.Extensions;

namespace DanmakuEngine.Engine.Platform.Linux.X11;

internal class X11Display : IDisposable
{
    private readonly IntPtr _display;

    public X11Display(string? display)
    {
        unsafe
        {
            _display = Xlib.XOpenDisplay(display is null ? null : display.ToCharPtr());
        }

        if (_display == IntPtr.Zero)
        {
            throw new X11Exception("Failed to open display");
        }
    }

    public void Dispose()
    {
        if (_display != IntPtr.Zero)
        {
            Xlib.XCloseDisplay(_display);
        }
    }

    public static implicit operator IntPtr(X11Display display)
        => display._display;
}
