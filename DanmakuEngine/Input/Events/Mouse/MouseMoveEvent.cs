using DanmakuEngine.Input.States;
using Silk.NET.Input;
using Silk.NET.Maths;

namespace DanmakuEngine.Input.Events.Mouse;

public class MouseMoveEvent : IMouseEvent
{
    /// <summary>
    /// The timestamp of the event.
    /// </summary>
    public uint Timestamp { get; set; }

    /// <summary>
    /// The device ID of the event.
    /// </summary>
    public uint DeviceId { get; set; }

    /// <summary>
    /// The delta of the mouse move.
    /// </summary>
    public Vector2D<float> Delta { get; set; }

    public bool Apply(InputState inputState)
    {
        if (Delta == Vector2D<float>.Zero)
            return false;

        inputState.Mouse.Position += Delta;
        inputState.Mouse.LastEvent = this;

        return true;
    }
}
