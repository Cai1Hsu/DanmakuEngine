using ImGuiNET;

namespace DanmakuEngine.DearImgui.Windowing;

public abstract class ImguiWindowBase(string title)
{
    // p_open
    private bool _visiable = true;
    public bool Visiable => _visiable;
    public string Title { get; } = title;
    public bool DoUpdate { get; set; } = true;
    public bool BypassBeginEnd { get; set; } = false;

    protected abstract void Update();

    internal void UpdateUI()
    {
        if (!DoUpdate)
        {
            _visiable = false;
            return;
        }

        if (BypassBeginEnd)
        {
            Update();
            return;
        }

        ImGui.Begin(Title, ref _visiable);
        {
            Update();
        }
        ImGui.End();
    }

    public void Register()
    {
        ImguiWindowManager.RegisterWindow(this);
        _visiable = true;
    }

    public void Unregister()
    {
        ImguiWindowManager.UnregisterWindow(this);
        _visiable = false;
    }
}
