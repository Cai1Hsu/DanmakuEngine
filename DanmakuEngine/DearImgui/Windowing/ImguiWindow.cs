using DanmakuEngine.DearImgui.Windowing;

namespace DanmakuEngine.DearImgui.Windowing;

public class ImguiWindow(string title)
    : ImguiWindowBase(title)
{
    public ImguiWindow()
        : this(string.Empty)
    {
        BypassBeginEnd = true;
    }

    public event Action? OnUpdate = null!;

    protected override void Update()
        => OnUpdate?.Invoke();
}
