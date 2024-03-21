using System.Runtime.InteropServices;

namespace DanmakuEngine.Engine.Platform.Linux.X11;

[StructLayout(LayoutKind.Sequential)]
internal unsafe struct XRROutputInfo
{
    public ulong timestamp;

    public ulong crtc;

    public char* name;

    public int nameLen;

    public ulong mm_width;
    public ulong mm_height;
    public ushort connection;
    public ushort subpixel_order;
    public int ncrtc;
    public ulong* crtcs;
    public int nclone;
    public ulong* clones;
    public int nmode;
    public int npreferred;
    public ulong* modes;
}
