using System.Runtime.InteropServices;

namespace DanmakuEngine.Engine.Platform.Linux.X11;

[StructLayout(LayoutKind.Sequential)]
internal unsafe struct XRRModeInfo
{
    public ulong id;
    public uint width;
    public uint height;
    public ulong dotClock;
    public uint hSyncStart;
    public uint hSyncEnd;
    public uint hTotal;
    public uint hSkew;
    public uint vSyncStart;
    public uint vSyncEnd;
    public uint vTotal;
    public char* name;
    public uint nameLength;
    public ulong modeFlags;
}
