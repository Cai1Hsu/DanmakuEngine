using System.Runtime.InteropServices;

namespace DanmakuEngine.Engine.Platform.Linux.X11;

internal static unsafe partial class Xlib
{
    [LibraryImport("libX11.so.6")]
    internal static partial IntPtr XOpenDisplay(char* display);

    [LibraryImport("libX11.so.6")]
    internal static partial void XCloseDisplay(IntPtr display);

    [LibraryImport("libX11.so.6")]
    internal static partial ulong XDefaultRootWindow(IntPtr display);

    [LibraryImport("libX11.so.6")]
    internal static partial int XFree(void* ptr);
}
