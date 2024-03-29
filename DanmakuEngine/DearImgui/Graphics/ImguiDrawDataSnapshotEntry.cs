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
            ImguiUtils.ImFree(CopyLists[i]->CmdBuffer.Data);
            ImguiUtils.ImFree(CopyLists[i]->VtxBuffer.Data);
            ImguiUtils.ImFree(CopyLists[i]->IdxBuffer.Data);
            ImguiUtils.ImFree(CopyLists[i]);
        }
        ImguiUtils.ImFree((nint)SrcLists);
        ImguiUtils.ImFree((nint)CopyLists);
        ImguiUtils.ImFree((nint)drawData.NativePtr);
    }
}
