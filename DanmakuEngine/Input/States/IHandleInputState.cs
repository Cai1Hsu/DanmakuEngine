using DanmakuEngine.Input.Events;

namespace DanmakuEngine.Input.States;

public interface IHandleInputState
{
    public void HandleInput(InputState inputState, IInputEvent e);
}
