using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SysNativeMemory = System.Runtime.InteropServices.NativeMemory;

namespace DanmakuEngine.Allocations;

public static unsafe class NativeMemory
{
    private static readonly bool _supportUcrt;

    static NativeMemory()
    {
        var windowsVersion = Environment.OSVersion.Version;

        _supportUcrt = !OperatingSystem.IsWindows() || (windowsVersion.Major > 6) || (windowsVersion.Major == 6 && windowsVersion.Minor > 2);
    }

    public static void* Alloc(UIntPtr byteCount)
    {
        if (_supportUcrt)
            return SysNativeMemory.Alloc(byteCount);

        return (void*)Marshal.AllocHGlobal((nint)byteCount);
    }

    public static void* Alloc(UIntPtr elementCount, UIntPtr elementSize)
    {
        if (_supportUcrt)
            return SysNativeMemory.Alloc(elementCount, elementSize);

        return (void*)Marshal.AllocHGlobal((nint)elementCount * (nint)elementSize);
    }

    public static void* AllocZeroed(UIntPtr byteCount)
    {
        if (_supportUcrt)
            return SysNativeMemory.AllocZeroed(byteCount);

        var ptr = (void*)Marshal.AllocHGlobal((nint)byteCount);
        Unsafe.InitBlockUnaligned(ptr, 0, (uint)byteCount);

        return ptr;
    }

    public static void* AllocZeroed(UIntPtr elementCount, UIntPtr elementSize)
    {
        if (_supportUcrt)
            return SysNativeMemory.AllocZeroed(elementCount, elementSize);

        nuint size = elementCount * elementSize;

        var ptr = (void*)Marshal.AllocHGlobal((nint)size);
        Unsafe.InitBlockUnaligned(ptr, 0, (uint)size);

        return ptr;
    }
    public static void* AlignedAlloc(UIntPtr byteCount, UIntPtr alignment)
        => SysNativeMemory.AlignedAlloc(byteCount, alignment);

    public static void AlignedFree(void* ptr)
        => SysNativeMemory.AlignedFree(ptr);

    public static void* AlignedRealloc(void* ptr, UIntPtr byteCount, UIntPtr alignment)
        => SysNativeMemory.AlignedRealloc(ptr, byteCount, alignment);

    public static void Free(void* ptr)
    {
        if (_supportUcrt)
            SysNativeMemory.Free(ptr);
        else
            Marshal.FreeHGlobal((nint)ptr);
    }

    public static void* Realloc(void* ptr, UIntPtr byteCount)
    {
        if (_supportUcrt)
            return SysNativeMemory.Realloc(ptr, byteCount);

        return (void*)Marshal.ReAllocHGlobal((IntPtr)ptr, (nint)byteCount);
    }

    public static void Clear(void* ptr, UIntPtr byteCount)
    {
        if (_supportUcrt)
        {
            SysNativeMemory.Clear(ptr, byteCount);
        }
        else
        {
            var block = new Span<byte>(ptr, (int)byteCount);
            block.Clear();
        }
    }

    public static void Fill(void* ptr, UIntPtr byteCount, byte value)
    {
        if (_supportUcrt)
            SysNativeMemory.Fill(ptr, byteCount, value);
        else
            Unsafe.InitBlockUnaligned(ref *(byte*)ptr, value, (uint)byteCount);
    }

    public static void Copy(void* src, void* dst, UIntPtr byteCount)
    {
        if (_supportUcrt)
        {
            SysNativeMemory.Copy(src, dst, byteCount);
        }
        else
        {
            var srcBlock = new Span<byte>(src, (int)byteCount);
            var dstBlock = new Span<byte>(dst, (int)byteCount);

            srcBlock.CopyTo(dstBlock);
        }
    }
}
