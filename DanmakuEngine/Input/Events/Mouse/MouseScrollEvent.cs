using DanmakuEngine.Input.States;
using Silk.NET.Maths;

namespace DanmakuEngine.Input.Events.Mouse;

public class MouseScrollEvent : IMouseEvent
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
    /// The delta of the scroll.
    /// </summary>
    public Vector2D<float> Delta { get; set; }

    /// <summary>
    /// Indicates whether the scroll is precise.
    /// </summary>
    public bool Precise { get; set; }

    // TODO: store the position of the mouse when the event was fired
    // public Vector2D<float> Position { get; set; }

    public bool Apply(InputState inputState)
    {
        inputState.Mouse.Scroll += Delta;
        inputState.Mouse.LastEvent = this;

        return true;
    }
}
