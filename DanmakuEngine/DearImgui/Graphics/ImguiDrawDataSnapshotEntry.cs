using System.Numerics;
using System.Runtime.InteropServices;
using ImGuiNET;

namespace DanmakuEngine.DearImgui.Graphics;

internal unsafe class ImguiDrawDataSnapshotEntry : IDisposable
{
    internal ImDrawList** SrcLists;
    internal ImDrawList** CopyLists;
    internal ImDrawList* _copyListsValue;
    internal int ListsCount;
    internal ImDrawDataPtr drawData;

    private bool _disposed;

    public ImguiDrawDataSnapshotEntry(int count)
    {
        this.ListsCount = count;
        this.SrcLists = (ImDrawList**)Marshal.AllocHGlobal(sizeof(ImDrawList*) * ListsCount);
        this.CopyLists = (ImDrawList**)Marshal.AllocHGlobal(sizeof(ImDrawList*) * ListsCount);
        this._copyListsValue = (ImDrawList*)Marshal.AllocHGlobal(sizeof(ImDrawList) * ListsCount);

        new Span<ImDrawList>(_copyListsValue, ListsCount).Clear();
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;

        Marshal.FreeHGlobal((nint)_copyListsValue);
        Marshal.FreeHGlobal((nint)drawData.NativePtr);
        Marshal.FreeHGlobal((nint)SrcLists);
        Marshal.FreeHGlobal((nint)CopyLists);
    }
}
