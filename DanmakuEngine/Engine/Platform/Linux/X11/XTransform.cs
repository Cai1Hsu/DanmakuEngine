using System.Runtime.InteropServices;
using Silk.NET.Maths;
using Vortice.Mathematics;

namespace DanmakuEngine.Engine.Platform.Linux.X11;

[StructLayout(LayoutKind.Sequential)]
internal struct XTransform
{
    public Matrix3X3<int> matrix;
}
