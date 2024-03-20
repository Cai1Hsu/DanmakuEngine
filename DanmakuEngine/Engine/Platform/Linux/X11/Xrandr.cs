using System.Runtime.InteropServices;

namespace DanmakuEngine.Engine.Platform.Linux.X11;

internal static unsafe partial class Xrandr
{
    [LibraryImport("libXrandr.so.2")]
    internal static partial XRRScreenResources* XRRGetScreenResources(IntPtr display, ulong window);

    [LibraryImport("libXrandr.so.2")]
    internal static partial void XRRFreeScreenResources(XRRScreenResources* resources);

    [LibraryImport("libXrandr.so.2")]
    internal static partial XRROutputInfo* XRRGetOutputInfo(IntPtr display, XRRScreenResources* resources, ulong output);

    [LibraryImport("libXrandr.so.2")]
    internal static partial void XRRFreeOutputInfo(XRROutputInfo* outputInfo);

    [LibraryImport("libXrandr.so.2")]
    internal static partial XRRCrtcInfo* XRRGetCrtcInfo(IntPtr display, XRRScreenResources* resources, ulong crtc);

    [LibraryImport("libXrandr.so.2")]
    internal static partial void XRRFreeCrtcInfo(XRRCrtcInfo* crtcInfo);

    [LibraryImport("libXrandr.so.2")]
    internal static partial int XRRGetCrtcTransform(IntPtr display, ulong crtc, XRRCrtcTransformAttributes** attributes);
}
