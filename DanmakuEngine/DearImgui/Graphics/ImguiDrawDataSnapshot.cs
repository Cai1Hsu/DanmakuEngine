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
    private int _remainedGCBuffers = 0;
    private bool requestedGC = false;

    internal ImguiDrawDataSnapshot()
    {
    }

    internal void ForceGC()
        => requestedGC = true;

    internal void WriteNewFrame(ImDrawDataPtr src)
    {
        if (!src.Valid)
            return;

        using (var usage = _cmdListsBuffer.GetForWrite())
        {
            var cmdListsCount = src.CmdListsCount;

            if (usage.Value is null)
                usage.Value = new(cmdListsCount);
            else
                usage.Value.PreTakeSnapShot(cmdListsCount);

            if (requestedGC)
            {
                _remainedGCBuffers = _cmdListsBuffer.Count;
                requestedGC = false;
            }

            if (_remainedGCBuffers > 0)
            {
                usage.Value.DoGC(true, true);
                --_remainedGCBuffers;
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
