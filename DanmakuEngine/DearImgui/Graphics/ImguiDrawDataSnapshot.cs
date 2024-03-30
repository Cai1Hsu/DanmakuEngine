using System.Collections.Concurrent;
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
    private bool requestedGC = false;
    private ConcurrentDictionary<int, bool> GCStatus = new();

    internal ImguiDrawDataSnapshot()
    {
    }

    internal void ForceGC()
        => requestedGC = true;

    internal void WriteNewFrame(ImDrawDataPtr src)
    {
        if (!src.Valid)
            return;

        if (requestedGC)
        {
            requestedGC = false;

            if (_cmdListsBuffer.GetUsingTypes().All(
                static ut => ut is UsingType.Avaliable))
            {
                _cmdListsBuffer.ExecuteOnAllBuffers(static (b, _) =>
                {
                    b?.DoGC(true, true);
                });
            }
        }

        using (var usage = _cmdListsBuffer.GetForWrite())
        {
            var cmdListsCount = src.CmdListsCount;

            if (usage.Value is null)
                usage.Value = new(cmdListsCount, usage.Index);
            else
                usage.Value.PreTakeSnapShot(cmdListsCount);

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
