using System.Diagnostics;
using DanmakuEngine.Logging;
using ImGuiNET;

namespace DanmakuEngine.DearImgui.Graphics;

/// <summary>
/// Represent a buffer that stores the ImDrawData and its ImDrawList(s)
/// </summary>
public unsafe class ImguiDrawDataBuffer : IDisposable
{
    private readonly Stopwatch _gcTimer = Stopwatch.StartNew();
    private readonly ImDrawData* _drawData;
    private int _bufferIndex;
    public int Capacity { get; private set; }
    public int Count { get; private set; }
    public ImDrawList** Lists { get; private set; }
    private bool _disposed = false;

    public int BufferIndex => _bufferIndex;
    public ImDrawDataPtr DrawData => new(_drawData);

    public ImguiDrawDataBuffer(int listsCount, int bufferIndex)
    {
        this._bufferIndex = bufferIndex;

        this.Count = listsCount;
        this.Capacity = listsCount;
        this._drawData = ImguiUtils.ImAlloc<ImDrawData>();

        if (Capacity == 0)
            return;

        this.Lists = (ImDrawList**)ImguiUtils.ImAlloc<nint>(Capacity);
    }

    public void DoGC()
    {
        // There is no need to do GC.
        if (Count == Capacity)
            return;

        for (int i = Count; i < Capacity; i++)
            ImguiUtils.ImFree(Lists[i]);

        reallocLists(Count);

        _gcTimer.Restart();

        Logger.Debug($"Did ImguiDrawDataBuffer GC, index: {_bufferIndex}");
    }

    public void PreTakeSnapShot(int newCount)
    {
        // enlarge the lists
        if (newCount > Capacity)
        {
            // disposeCopyListsData();
            reallocLists(newCount);

            // Alloc for new lists
            for (int i = Count; i < Capacity; i++)
                Lists[i] = ImguiUtils.ImAlloc<ImDrawList>();
        }
        else if ((Capacity > newCount * 2)
                 && _gcTimer.ElapsedMilliseconds > 1000)
        {
            Count = newCount;

            // Some times there are transient heavy scenes that allocates a lot of memory
            // We want to free these memory to prevent too much memory from being wasted.
            DoGC();

            return;
        }

        Count = newCount;
    }

    private void reallocLists(int newCapacity)
    {
        Lists = (ImDrawList**)ImguiUtils.ImRealloc<nint>((nint)Lists, newCapacity);
        Capacity = newCapacity;
    }

    public void SnapDrawData(ImDrawDataPtr src)
    {
        ImDrawData* p_src = src.NativePtr;

        // step1: Copy properties except CmdLists
        ImVector backup_draw_list = new();
        backup_draw_list.Swap(ref p_src->CmdLists);
        {
            Debug.Assert(p_src->CmdLists.Data is 0);
            *_drawData = *p_src;
        }
        backup_draw_list.Swap(ref p_src->CmdLists);

        // step2: Swap the CmdLists(and ownership)
        for (var i = 0; i < p_src->CmdListsCount; i++)
        {
            ImDrawList* src_list = ((ImDrawList**)p_src->CmdLists.Data)[i];

            Lists[i]->_Data = src_list->_Data;

            src_list->CmdBuffer.Swap(ref Lists[i]->CmdBuffer);
            src_list->VtxBuffer.Swap(ref Lists[i]->VtxBuffer);
            src_list->IdxBuffer.Swap(ref Lists[i]->IdxBuffer);
        }

        *&_drawData->CmdLists.Data = (nint)Lists;
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;

        for (int i = 0; i < Capacity; i++)
            ImguiUtils.ImFree(Lists[i]);

        ImguiUtils.ImFree(Lists);
        ImguiUtils.ImFree(_drawData);

        Logger.Debug($"Disposed ImguiDrawDataBuffer, index: {_bufferIndex}");
    }
}
