using DanmakuEngine.Dependency;
using DanmakuEngine.Engine;

namespace DanmakuEngine.Input.Handlers;

public interface IInputHandler : IInjectable
{
    public void Register(GameHost host);
}
