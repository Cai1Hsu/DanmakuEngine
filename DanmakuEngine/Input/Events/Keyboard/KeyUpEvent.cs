using DanmakuEngine.Input.States;
using Silk.NET.Input;

namespace DanmakuEngine.Input.Events.Mouse;

public class KeyUpEvent : IKeyEvent
{
    /// <summary>
    /// See <see cref="IKeyEvent.DeviceId"/>.
    /// Currently unused.
    /// </summary>
    public uint DeviceId { get; set; }

    public uint Timestamp { get; set; }

    public Keys Button { get; set; }

    public bool IsRepeatInput { get; set; }

    public bool Apply(InputState inputState, IHandleInputState handler)
    {
        inputState.Keyboard.LastEvent = this;
        return inputState.Keyboard.SetPressed(Button, false);
    }
}
