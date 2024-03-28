using System.Diagnostics;
using System.Runtime.InteropServices;
using DanmakuEngine.Allocations;
using DanmakuEngine.Logging;
using ImGuiNET;
using Silk.NET.SDL;
using Vortice.DXGI;

namespace DanmakuEngine.DearImgui.Graphics;

public unsafe class ImguiDrawDataSnapshot
{
    private TripleBuffer<ImguiDrawDataSnapshotEntry> _cmdListsBuffer = new();

    internal ImguiDrawDataSnapshot()
    {
        _cmdListsBuffer.OnObjectOverwritten += freeOverwritten;
    }

    private static void freeOverwritten(ImguiDrawDataSnapshotEntry? obj)
    {
        if (obj is null)
            return;

        obj.Dispose();
    }

    internal void WriteNewFrame(ImDrawDataPtr src)
    {
        var p_src = src.NativePtr;

        if (!src.Valid)
            return;

        ImDrawData* newData = (ImDrawData*)Marshal.AllocHGlobal(sizeof(ImDrawData));

        if (p_src->CmdLists.Data != 0)
            Debugger.Break();

        // Snap using swap
        ImVector backup_draw_list = new();
        backup_draw_list.Swap(ref p_src->CmdLists);
        Debug.Assert(p_src->CmdLists.Data == 0);
        *newData = *p_src;
        backup_draw_list.Swap(ref p_src->CmdLists);

        ImguiDrawDataSnapshotEntry entry = new(p_src->CmdListsCount);

        for (var i = 0; i < p_src->CmdListsCount; i++)
        {
            ImDrawList* src_list = src.CmdLists[i].NativePtr;

            entry.SrcLists[i] = src_list;

            entry.CopyLists[i] = entry._copyListsValue + i * sizeof(ImDrawList);
            entry.CopyLists[i]->_Data = src_list->_Data;

            Debug.Assert(entry.SrcLists[i] == src_list);

            entry.SrcLists[i]->CmdBuffer.Swap(ref entry.CopyLists[i]->CmdBuffer);
            entry.SrcLists[i]->VtxBuffer.Swap(ref entry.CopyLists[i]->VtxBuffer);
            entry.SrcLists[i]->IdxBuffer.Swap(ref entry.CopyLists[i]->IdxBuffer);

            entry.SrcLists[i]->CmdBuffer.Reserve<ImDrawCmd>(entry.CopyLists[i]->CmdBuffer.Capacity);
            entry.SrcLists[i]->VtxBuffer.Reserve<ImDrawVert>(entry.CopyLists[i]->VtxBuffer.Capacity);
            entry.SrcLists[i]->IdxBuffer.Reserve<ushort>(entry.CopyLists[i]->IdxBuffer.Capacity);
        }

        *&newData->CmdLists.Data = (nint)entry.CopyLists;
        entry.drawData = newData;

        using (var usage = _cmdListsBuffer.GetForWrite())
            usage.Value = entry;
    }

    internal UsageValue<ImguiDrawDataSnapshotEntry>? GetForRead()
        => _cmdListsBuffer.GetForRead();
}
