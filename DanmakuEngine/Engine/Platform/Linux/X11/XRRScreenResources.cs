using System.Runtime.InteropServices;

namespace DanmakuEngine.Engine.Platform.Linux.X11;

[StructLayout(LayoutKind.Sequential)]
internal unsafe struct XRRScreenResources
{
    public ulong timestamp;
    public ulong configTimestamp;
    public int ncrtc;
    public ulong* crtcs;
    public int noutput;
    public ulong* outputs;
    public int nmode;
    public XRRModeInfo* modes;
}
