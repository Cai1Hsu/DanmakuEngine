using ImGuiNET;

namespace DanmakuEngine.DearImgui;

internal readonly ref struct ImguiContextState
{
    private readonly nint _newContext;
    private readonly nint _oldContext;

    internal ImguiContextState(nint newContext)
    {
        _oldContext = ImGui.GetCurrentContext();
        _newContext = newContext;

        if (_oldContext != _newContext && _oldContext != 0)
            ImGui.SetCurrentContext(newContext);
    }

    public void Dispose()
    {
        if (_oldContext != _newContext && _oldContext != 0)
            ImGui.SetCurrentContext(_oldContext);
    }
}
