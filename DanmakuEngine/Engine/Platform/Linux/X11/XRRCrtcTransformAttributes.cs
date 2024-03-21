using System.Runtime.InteropServices;

namespace DanmakuEngine.Engine.Platform.Linux.X11;

[StructLayout(LayoutKind.Sequential)]
internal unsafe struct XRRCrtcTransformAttributes
{
    public XTransform pendingTransform;
    public char* pendingFilter;
    public int pendingNparams;
    public IntPtr pendingParams;
    public XTransform currentTransform;
    public char* currentFilter;
    public int currentNparams;
    public IntPtr currentParams;
}
