using DanmakuEngine.Input.Events.Keyboard;
using DanmakuEngine.Input.States;

namespace DanmakuEngine.Input.Keybards;

public partial class KeyboardState
{
    private readonly ButtonStates<Keys> _states = new();

    public IKeyEvent? LastEvent { get; internal set; }

    public bool IsPressed(Keys key)
        => _states.IsPressed(key);

    public bool SetPressed(Keys key, bool pressed)
        => _states.SetPressed(key, pressed);

    /// <summary>
    /// Indicate whether the control key is pressed.
    /// </summary>
    public bool CtrlModifier => IsPressed(Keys.ControlLeft) || IsPressed(Keys.ControlRight);

    /// <summary>
    /// Indicate whether the alt key is pressed.
    /// </summary>
    public bool AltModifier => IsPressed(Keys.AltLeft) || IsPressed(Keys.AltRight);

    /// <summary>
    /// Indicate whether the shift key is pressed.
    /// </summary>
    public bool ShiftModifier => IsPressed(Keys.ShiftLeft) || IsPressed(Keys.ShiftRight);

    /// <summary>
    /// Indicate whether the super key is pressed.
    /// On Windows, this is the Windows key.
    /// </summary>
    public bool SuperModifier => IsPressed(Keys.SuperLeft) || IsPressed(Keys.SuperRight);

    public void Initialize()
    {
        _states.Clear();
    }
}
