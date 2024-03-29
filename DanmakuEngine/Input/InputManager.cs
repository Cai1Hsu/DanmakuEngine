using DanmakuEngine.Engine;
using DanmakuEngine.Input.Handlers;
using DanmakuEngine.Logging;

namespace DanmakuEngine.Input;

public class InputManager
{
    public IInputHandler[] Handlers { get; private set; }

    public InputManager()
    {
        Handlers = new[]
        {
            new HostKeyboardHandler(),
        };
    }

    public void RegisterHandlers(GameHost host)
    {
        foreach (var h in Handlers)
        {
            Logger.Debug($"Registering {h.GetType()}");

            h.AutoInject();

            h.Register(host);
        }
    }
}
