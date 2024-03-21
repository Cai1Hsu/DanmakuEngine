using System.Runtime.InteropServices;

namespace DanmakuEngine.Engine.Platform.Linux.X11;

[StructLayout(LayoutKind.Sequential)]
internal unsafe struct XRRCrtcInfo
{
    public ulong timestamp;
    public int x, y;
    public uint width, height;
    public ulong mode;
    public ushort rotation;
    public int noutput;
    public ulong* outputs;
    public ushort rotations;
    public int npossible;
    public ulong* possible;
}
