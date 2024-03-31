using System.Diagnostics;
using DanmakuEngine.Logging;
using ImGuiNET;

namespace DanmakuEngine.DearImgui.Graphics;

/// <summary>
/// Represent a buffer that stores the ImDrawData and its ImDrawList(s)
/// </summary>
internal unsafe class ImguiDrawDataBuffer : IDisposable
{
    private readonly Stopwatch _gcTimer = Stopwatch.StartNew();
    private readonly ImDrawData* _drawData;
    private int _bufferIndex;
    private int _capacity;
    internal int Count { get; private set; }
    internal ImDrawList** Lists { get; private set; }
    private bool _disposed = false;

    internal int BufferIndex => _bufferIndex;
    internal ImDrawDataPtr DrawData => new(_drawData);

    public ImguiDrawDataBuffer(int listsCount, int bufferIndex)
    {
        this._bufferIndex = bufferIndex;

        this.Count = listsCount;
        this._capacity = listsCount;
        this._drawData = ImguiUtils.ImAlloc<ImDrawData>();

        if (_capacity == 0)
            return;

        this.Lists = (ImDrawList**)ImguiUtils.ImAlloc<nint>(_capacity);
    }

    internal void DoGC()
    {
        for (int i = Count; i < _capacity; i++)
            ImguiUtils.ImFree(Lists[i]);

        reallocLists(Count);

        _gcTimer.Restart();

        Logger.Debug($"Did ImguiDrawDataBuffer GC, index: {_bufferIndex}");
    }

    internal void PreTakeSnapShot(int newCount)
    {
        // enlarge the lists
        if (newCount > _capacity)
        {
            // disposeCopyListsData();
            reallocLists(newCount);

            // Alloc for new lists
            for (int i = Count; i < _capacity; i++)
                Lists[i] = ImguiUtils.ImAlloc<ImDrawList>();
        }
        else if ((_capacity > newCount * 2)
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
        _capacity = newCapacity;
    }

    internal void SnapDrawData(ImDrawDataPtr src)
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

        for (int i = 0; i < _capacity; i++)
            ImguiUtils.ImFree(Lists[i]);

        ImguiUtils.ImFree(Lists);
        ImguiUtils.ImFree(_drawData);

        Logger.Debug($"Disposed ImguiDrawDataBuffer, index: {_bufferIndex}");
    }
}
