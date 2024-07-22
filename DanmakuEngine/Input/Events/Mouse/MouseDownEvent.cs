using DanmakuEngine.Input.States;
using Silk.NET.Maths;

namespace DanmakuEngine.Input.Events.Mouse;

public class MouseDownEvent : IMouseEvent
{
    public uint DeviceId { get; set; }

    public uint Timestamp { get; set; }

    public MouseButton Button { get; set; }

    public bool IsDoubleClick { get; set; }

    // TODO: store the position of the mouse when the event was fired
    // public Vector2D<float> Position { get; set; }

    public bool Apply(InputState inputState)
    {
        inputState.Mouse.LastEvent = this;
        return inputState.Mouse.SetPressed(Button, true);
    }
}
