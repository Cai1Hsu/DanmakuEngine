#if !DEBUG
using System.Runtime.CompilerServices;
#endif
using DanmakuEngine.Allocations;
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
        var ptr = NativeMemory.AllocZeroed((nuint)size);

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
        NativeMemory.Free((void*)address);

#if DEBUG
        RemoveRecord(address);
        ++freedCount;
#endif

    }

#if !DEBUG
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static T* ImRealloc<T>(nint address, int count = 1)
        where T : unmanaged
        => (T*)ImRealloc(address, count * sizeof(T));

#if !DEBUG
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static void* ImRealloc(nint address, int size)
    {
        var ptr = NativeMemory.Realloc((void*)address, (nuint)size);

#if DEBUG
        RemoveRecord(address);
        AddRecord(new((nint)ptr, (uint)size, AllocCallsite.Realloc));
        ++reallocCount;
#endif
        return ptr;
    }
}
