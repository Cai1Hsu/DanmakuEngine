using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace DanmakuEngine.DearImgui;

public static unsafe class ImguiUtils
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static T* ImAlloc<T>(int count = 1)
        where T : unmanaged
        => (T*)ImAlloc(sizeof(T) * count);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void* ImAlloc(int size)
    {
        void* ptr = (void*)Marshal.AllocHGlobal(size);
        Unsafe.InitBlockUnaligned(ptr, 0, (uint)size);
        return ptr;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ImFree(void* ptr)
        => ImFree((nint)ptr);

    internal static void ImFree(nint address)
        => Marshal.FreeHGlobal(address);
}
