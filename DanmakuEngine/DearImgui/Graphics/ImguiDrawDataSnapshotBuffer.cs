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
        this.ListsCapacity = count;
        this.ListsCount = count;
        this.SrcLists = (ImDrawList**)ImguiUtils.ImAlloc(sizeof(ImDrawList*) * ListsCount);
        this.CopyLists = (ImDrawList**)ImguiUtils.ImAlloc(sizeof(ImDrawList*) * ListsCount);
        this._drawData = ImguiUtils.ImAlloc<ImDrawData>();
    }

    private void disposeAllResevedData()
    {
        for (int i = 0; i < ListsCapacity; i++)
        {
            ImguiUtils.ImFree(CopyLists[i]->CmdBuffer.Data);
            ImguiUtils.ImFree(CopyLists[i]->VtxBuffer.Data);
            ImguiUtils.ImFree(CopyLists[i]->IdxBuffer.Data);
            ImguiUtils.ImFree(CopyLists[i]);
        }
    }

    private void disposeOverritenData()
    {
        // These should be freed by ImGui
        // but we took their ownership in order to supprot multithread rendering.
        // So we must free them.
        for (int i = 0; i < ListsCount; i++)
        {
            ImguiUtils.ImFree(CopyLists[i]->CmdBuffer.Data);
            ImguiUtils.ImFree(CopyLists[i]->VtxBuffer.Data);
            ImguiUtils.ImFree(CopyLists[i]->IdxBuffer.Data);
            ImguiUtils.ImFree(CopyLists[i]);
        }
    }

    internal void PreTakeSnapShot(ImDrawDataPtr src)
    {
        var newCount = src.CmdListsCount;

        if (newCount > ListsCapacity)
        {
            disposeOverritenData();

            ListsCount = newCount;

            // enlarge the lists
            reallocLists();
        }
        else
        {
            // Some times there are transient heavy scenes that allocates a lot of memory
            // We want to free these memory to prevent too much memory from being wasted.
            if (ListsCapacity > ListsCount * 2  // Only do this when we allocated too much.
                && _gcTimer.ElapsedMilliseconds > 1000)
            {
                disposeAllResevedData();

                ListsCount = newCount;
                // shrink the lists
                reallocLists();
                _gcTimer.Restart();

                Logger.Debug($"Did ImguiDrawDataSnapshotBuffer GC");
            }
            else
            {
                disposeOverritenData();
                ListsCount = newCount;
            }
        }
    }

    private void reallocLists()
    {
        SrcLists = (ImDrawList**)ImguiUtils.ImRealloc((nint)SrcLists, sizeof(ImDrawList*) * ListsCount);
        CopyLists = (ImDrawList**)ImguiUtils.ImRealloc((nint)CopyLists, sizeof(ImDrawList*) * ListsCount);
        ListsCapacity = ListsCount;
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

            // TODO
            // Since the CopyLists is managed by us, we should try to avoid frequent alloc and free operations
            CopyLists[i] = ImguiUtils.ImAlloc<ImDrawList>();
            CopyLists[i]->_Data = src_list->_Data;

            Debug.Assert(SrcLists[i] == src_list);

            SrcLists[i]->CmdBuffer.Swap(ref CopyLists[i]->CmdBuffer);
            SrcLists[i]->VtxBuffer.Swap(ref CopyLists[i]->VtxBuffer);
            SrcLists[i]->IdxBuffer.Swap(ref CopyLists[i]->IdxBuffer);
        }

        *&_drawData->CmdLists.Data = (nint)CopyLists;
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;

        disposeAllResevedData();

        ImguiUtils.ImFree(SrcLists);
        ImguiUtils.ImFree(CopyLists);
        ImguiUtils.ImFree(_drawData);

        Logger.Debug($"Disposed ImguiDrawDataSnapshotBuffer");
    }
}
