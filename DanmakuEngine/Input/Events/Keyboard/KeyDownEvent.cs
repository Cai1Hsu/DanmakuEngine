using DanmakuEngine.Input.States;

namespace DanmakuEngine.Input.Events.Keyboard;

public class KeyDownEvent : IKeyEvent
{
    /// <summary>
    /// See <see cref="IKeyEvent.DeviceId"/>.
    /// Currently unused.
    /// </summary>
    public uint DeviceId { get; set; }

    public uint Timestamp { get; set; }

    public Keys Button { get; set; }

    public bool IsRepeatInput { get; set; }

    public bool Apply(InputState inputState)
    {
        inputState.Keyboard.LastEvent = this;
        return inputState.Keyboard.SetPressed(Button, true);
    }
}
