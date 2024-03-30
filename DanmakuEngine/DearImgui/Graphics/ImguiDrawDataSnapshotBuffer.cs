using System.Diagnostics;
using System.Runtime.CompilerServices;
using DanmakuEngine.Logging;
using ImGuiNET;

namespace DanmakuEngine.DearImgui.Graphics;

internal unsafe class ImguiDrawDataSnapshotBuffer : IDisposable
{
    private int ListsCapacity;
    internal int ListsCount;
    internal ImDrawList** SrcLists;
    internal ImDrawList** CopyLists;
    private ImDrawData* _drawData;
    private readonly Stopwatch _gcTimer = Stopwatch.StartNew();
    private bool _disposed = false;
    internal ImDrawDataPtr DrawData => new(_drawData);

    public ImguiDrawDataSnapshotBuffer(int count)
    {
        this.ListsCount = count;
        this.ListsCapacity = count;
        this._drawData = ImguiUtils.ImAlloc<ImDrawData>();

        reallocLists();
    }

    internal void PreTakeSnapShot(int newCount)
    {
        // Some times there are transient heavy scenes that allocates a lot of memory
        // We want to free these memory to prevent too much memory from being wasted.
        var doGC = (ListsCapacity > newCount * 2)
                 && _gcTimer.ElapsedMilliseconds > 1000;

        if (newCount > ListsCapacity || doGC)
        {
            disposeCopyListsData();

            ListsCount = newCount;

            // enlarge or shrink the lists
            reallocLists();

            if (doGC)
            {
                // Did GC in `reallocLists()`
                _gcTimer.Restart();

                Logger.Debug($"Did ImguiDrawDataSnapshotBuffer GC");
            }
        }
        else
        {
            ListsCount = newCount;
        }
    }

    private void reallocLists()
    {
        SrcLists = (ImDrawList**)ImguiUtils.ImRealloc((nint)SrcLists, sizeof(ImDrawList*) * ListsCount);
        CopyLists = (ImDrawList**)ImguiUtils.ImRealloc((nint)CopyLists, sizeof(ImDrawList*) * ListsCount);
        ListsCapacity = ListsCount;

        for (int i = 0; i < ListsCapacity; i++)
            CopyLists[i] = ImguiUtils.ImAlloc<ImDrawList>();
    }

    internal void SnapDrawData(ImDrawDataPtr src)
    {
        ImDrawData* p_src = src.NativePtr;

        // Snap using swap
        ImVector backup_draw_list = new();
        backup_draw_list.Swap(ref p_src->CmdLists);
        Debug.Assert(p_src->CmdLists.Data == 0);
        *_drawData = *p_src;
        backup_draw_list.Swap(ref p_src->CmdLists);

        for (var i = 0; i < p_src->CmdListsCount; i++)
        {
            ImDrawList* src_list = src.CmdLists[i].NativePtr;

            SrcLists[i] = src_list;
            CopyLists[i]->_Data = src_list->_Data;

            Debug.Assert(SrcLists[i] == src_list);

            SrcLists[i]->CmdBuffer.Swap(ref CopyLists[i]->CmdBuffer);
            SrcLists[i]->VtxBuffer.Swap(ref CopyLists[i]->VtxBuffer);
            SrcLists[i]->IdxBuffer.Swap(ref CopyLists[i]->IdxBuffer);
        }

        *&_drawData->CmdLists.Data = (nint)CopyLists;
    }

    private void disposeCopyListsData()
    {
        for (int i = 0; i < ListsCapacity; i++)
        {
            ImguiUtils.ImFree(CopyLists[i]->CmdBuffer.Data);
            ImguiUtils.ImFree(CopyLists[i]->VtxBuffer.Data);
            ImguiUtils.ImFree(CopyLists[i]->IdxBuffer.Data);
            ImguiUtils.ImFree(CopyLists[i]);
        }
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;

        disposeCopyListsData();

        ImguiUtils.ImFree(SrcLists);
        ImguiUtils.ImFree(CopyLists);
        ImguiUtils.ImFree(_drawData);

        Logger.Debug($"Disposed ImguiDrawDataSnapshotBuffer");
    }
}
