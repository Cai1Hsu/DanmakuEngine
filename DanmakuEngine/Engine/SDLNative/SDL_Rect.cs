using System.Runtime.InteropServices;
using Silk.NET.Maths;

namespace DanmakuEngine.Engine.SDLNative;

[StructLayout(LayoutKind.Sequential)]
public unsafe struct SDL_Rect
{
    public Vector2D<int> Position;
    public Vector2D<int> Size;
}
