using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using ImGuiNET;

namespace DanmakuEngine.DearImgui.Graphics;

internal unsafe class ImguiDrawDataSnapshotEntry : IDisposable
{
    internal ImDrawList** SrcLists;
    internal ImDrawList** CopyLists;
    internal int ListsCount;
    internal ImDrawDataPtr drawData;

    private bool _disposed;

    public ImguiDrawDataSnapshotEntry(int count)
    {
        this.ListsCount = count;
        this.SrcLists = (ImDrawList**)ImguiUtils.ImAlloc(sizeof(ImDrawList*) * ListsCount);
        this.CopyLists = (ImDrawList**)ImguiUtils.ImAlloc(sizeof(ImDrawList*) * ListsCount);
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;

        for (var i = 0; i < ListsCount; i++)
        {
            Marshal.FreeHGlobal((nint)CopyLists[i]);
        }
        Marshal.FreeHGlobal((nint)SrcLists);
        Marshal.FreeHGlobal((nint)CopyLists);
        Marshal.FreeHGlobal((nint)drawData.NativePtr);
    }
}
