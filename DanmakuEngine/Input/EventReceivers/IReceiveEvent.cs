using DanmakuEngine.Dependency;

namespace DanmakuEngine.Input.EventReceivers;

public interface IReceiveEvent
{
    public bool Active { get; }

    public uint Priority { get; }

    public InputManager InputManager { get; set; }
}
