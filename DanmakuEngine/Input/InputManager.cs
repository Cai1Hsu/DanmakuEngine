using DanmakuEngine.Input.Handlers;
using DanmakuEngine.Dependency;
using DanmakuEngine.Engine;
using Silk.NET.Input;
using DanmakuEngine.Logging;

namespace DanmakuEngine.Input;

public class InputManager
{
    public List<IInputHandler> Handlers { get; private set; } = null!;

    public InputManager()
    {
        Handlers = new List<IInputHandler>()
        {
            new TopKeyboardHandler(),
        };

        foreach (var h in Handlers)
            h.AutoInject();
    }

    public void RegisterHandlers(GameHost host)
    {
        foreach (var h in Handlers)
        {
            Logger.Debug($"Registering {h.GetType()}");

            h.Register(host);
        }
    }
}
