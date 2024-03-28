using ImGuiNET;

namespace DanmakuEngine.DearImgui.Windowing;

public class ImguiDemoWindow : ImguiWindowBase
{
    public ImguiDemoWindow()
        : base(string.Empty)
    {
        BypassBeginEnd = true;
    }

    protected override void Update()
        => ImGui.ShowDemoWindow();
}
