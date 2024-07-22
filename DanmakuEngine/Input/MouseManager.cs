using DanmakuEngine.Bindables;
using DanmakuEngine.Input.Handlers;

namespace DanmakuEngine.Input;

public class MouseManager
{
    /// <summary>
    /// Whether to use raw input.
    /// </summary>
    public Bindable<bool> RawInput { get; private set; } = null!;

    /// <summary>
    /// Whether to show the OS cursor.
    /// </summary>
    public Bindable<bool> ShowCursor { get; private set; } = null!;

    /// <summary>
    /// Will ignore all mouse move event if set to true.
    /// </summary>
    public Bindable<bool> LockCursor { get; private set; } = null!;

    public Bindable<bool> InvertVertical { get; private set; } = null!;

    // I know some people like inverting vertical, but will anyone like this?
    public Bindable<bool> InvertHorizontal { get; private set; } = null!;

    /// <summary>
    /// Whether to capture the mouse input in Relative mode.
    /// </summary>
    public Bindable<bool> RelativeMode { get; private set; } = null!;

    /// <summary>
    /// Indicates whether the cursor is in the window. DO NOT CHANGE THIS VALUE.
    /// </summary>
    public Bindable<bool> CursorInWindow { get; private set; } = null!;

    public MouseHandler Initialize(MouseHandler mouseHandler)
    {
        RawInput = mouseHandler.RawInput.GetBoundCopy();
        ShowCursor = mouseHandler.ShowCursor.GetBoundCopy();
        LockCursor = mouseHandler.LockCursor.GetBoundCopy();
        InvertVertical = mouseHandler.InvertVertical.GetBoundCopy();
        InvertHorizontal = mouseHandler.InvertHorizontal.GetBoundCopy();
        RelativeMode = mouseHandler.UseRelativeMode.GetBoundCopy();
        CursorInWindow = mouseHandler.CursorInWindow.GetBoundCopy();

        return mouseHandler;
    }

    public MouseManager()
    {
    }
}
