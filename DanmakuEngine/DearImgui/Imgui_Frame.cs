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
    private static ImguiDrawDataSnapshot _drawDataSnapshotBuffer = new();

    public static void ForceCollectSnapshots()
    {
        _drawDataSnapshotBuffer.ForceGC();
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

        _drawDataSnapshotBuffer.WriteNewFrame(ImGui.GetDrawData());
    }

    internal static void Render()
    {
        if (!_initialized)
            return;

        var buffer = _drawDataSnapshotBuffer.GetForRead();

        if (buffer != null)
        {
            using (buffer)
            {
                if (buffer.Value is not null)
                    drawImGui(buffer.Value);
            }
        }
    }
}
