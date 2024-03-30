using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace DanmakuEngine.DearImgui;

public static unsafe partial class ImguiUtils
{
#if !DEBUG
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static T* ImAlloc<T>(int count = 1)
        where T : unmanaged
        => (T*)ImAlloc(sizeof(T) * count);

#if !DEBUG
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static void* ImAlloc(int size)
    {
        void* ptr = (void*)Marshal.AllocHGlobal(size);
        Unsafe.InitBlockUnaligned(ptr, 0, (uint)size);

#if DEBUG
        AddRecord(new((nint)ptr, (uint)size, AllocCallsite.Alloc));
        ++allcatedCount;
#endif
        return ptr;
    }

#if !DEBUG
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static void ImFree(void* ptr)
        => ImFree((nint)ptr);

#if !DEBUG
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static void ImFree(nint address)
    {
        Marshal.FreeHGlobal(address);

#if DEBUG
        RemoveRecord(address);
        ++freedCount;
#endif

    }

#if !DEBUG
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static void ImRealloc<T>(nint address, int count = 1)
        where T : unmanaged
        => ImRealloc(address, count * sizeof(T));

#if !DEBUG
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static void* ImRealloc(nint address, int size)
    {
        var ptr = (void*)Marshal.ReAllocHGlobal(address, size);
        Unsafe.InitBlockUnaligned(ptr, 0, (uint)size);

#if DEBUG
        AddRecord(new((nint)ptr, (uint)size, AllocCallsite.Realloc));
        RemoveRecord(address);
        ++reallocCount;
#endif

        return ptr;
    }
}
