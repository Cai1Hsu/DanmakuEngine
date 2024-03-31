using System.Diagnostics;
using DanmakuEngine.Allocations;
using DanmakuEngine.DearImgui.Graphics;
using DanmakuEngine.DearImgui.Windowing;
using DanmakuEngine.Logging;
using DanmakuEngine.Timing;
using ImGuiNET;

namespace DanmakuEngine.DearImgui;

public static partial class Imgui
{
    private static TripleBuffer<ImguiDrawDataBuffer> _drawDataBuffers = new();
    private static bool requestedGC = false;

    public static void ForceCollectSnapshots()
    {
        requestedGC = true;
    }

    internal static void Update()
    {
        if (!_initialized)
            return;

        ImGui.NewFrame();

        _io = ImGui.GetIO();
        _io.DeltaTime = Time.UpdateDeltaF;

        if (_dockingEnabled)
            ImGui.DockSpaceOverViewport();

        ImguiWindowManager.UpdateWindows();
        OnUpdate?.Invoke();

        ImGui.Render();

        bufferNewFrame(ImGui.GetDrawData());
    }

    private static void bufferNewFrame(ImDrawDataPtr src)
    {
        if (requestedGC)
        {
            requestedGC = false;

            if (_drawDataBuffers.GetUsingTypes().All(
                static ut => ut is UsingType.Avaliable))
            {
                _drawDataBuffers.ExecuteOnAllBuffers(static (b, _) =>
                {
                    b?.DoGC();
                });
            }
            else
            {
                Logger.Warn("Some buffers were not avaliable for GC");
            }
        }

        if (!src.Valid)
            return;

        using (var usage = _drawDataBuffers.GetForWrite())
        {
            var cmdListsCount = src.CmdListsCount;

            if (usage.Value is null)
                usage.Value = new(cmdListsCount, usage.Index);
            else
                usage.Value.PreTakeSnapShot(cmdListsCount);

            usage.Value.SnapDrawData(src);
        }
    }

    internal static void Render()
    {
        if (!_initialized)
            return;

        using var buffer = _drawDataBuffers.GetForRead();

        if (buffer != null
            && buffer.Value is not null)
        {
            drawImGui(buffer.Value);
        }
    }
}
