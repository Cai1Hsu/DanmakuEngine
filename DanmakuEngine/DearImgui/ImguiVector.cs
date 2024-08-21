using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using ImGuiNET;

namespace DanmakuEngine.DearImgui;

[StructLayout(LayoutKind.Sequential)]
public struct ImguiVector
{
    public int Size;

    public int Capacity;

    public nint Data;

    public static unsafe explicit operator ImVector(ImguiVector v)
        => *(ImVector*)&v;

    public static unsafe explicit operator ImguiVector(ImVector v)
        => *(ImguiVector*)&v;
}

[StructLayout(LayoutKind.Sequential)]
public struct ImguiVector<T>
{
    public int Size;

    public int Capacity;

    public nint Data;

    public unsafe ref T this[int index]
        => ref Unsafe.AsRef<T>((byte*)(void*)Data + (index * Unsafe.SizeOf<T>()));

    public static unsafe explicit operator ImVector(ImguiVector<T> v)
        => *(ImVector*)&v;

    public static unsafe explicit operator ImguiVector<T>(ImVector v)
        => *(ImguiVector<T>*)&v;
}
