using DanmakuEngine.Input.States;

namespace DanmakuEngine.Input.Events;

public interface IInputEvent
{
    /// <summary>
    /// The time the event was created. From the SDL, not the game engine.
    /// </summary>
    public uint Timestamp { get; }

    public bool Apply(InputState inputState);
}
