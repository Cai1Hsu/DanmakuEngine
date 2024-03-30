using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using DanmakuEngine.Allocations;
using DanmakuEngine.Logging;
using ImGuiNET;

namespace DanmakuEngine.DearImgui.Graphics;

public unsafe class ImguiDrawDataSnapshot : IDisposable
{
    private TripleBuffer<ImguiDrawDataSnapshotBuffer> _cmdListsBuffer = new();

    internal ImguiDrawDataSnapshot()
    {
    }

    internal void WriteNewFrame(ImDrawDataPtr src)
    {
        if (!src.Valid)
            return;

        using (var usage = _cmdListsBuffer.GetForWrite())
        {
            if (usage.Value is null)
            {
                usage.Value = new(src.CmdListsCount);
            }
            else
            {
                usage.Value.PreTakeSnapShot(src);
            }

            usage.Value.SnapDrawData(src);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal UsageValue<ImguiDrawDataSnapshotBuffer>? GetForRead()
        => _cmdListsBuffer.GetForRead();

    public void Dispose()
    {
        _cmdListsBuffer.Dispose();
    }
}
