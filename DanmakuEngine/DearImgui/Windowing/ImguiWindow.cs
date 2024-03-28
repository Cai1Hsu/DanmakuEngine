using DanmakuEngine.DearImgui.Windowing;

namespace DanmakuEngine.DearImgui.Windowing;

public class ImguiWindow(string title)
    : ImguiWindowBase(title)
{
    public event Action? OnUpdate = null!;

    protected override void Update()
        => OnUpdate?.Invoke();
}
