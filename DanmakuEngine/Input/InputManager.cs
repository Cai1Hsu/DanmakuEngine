using DanmakuEngine.Engine;
using DanmakuEngine.Games;
using DanmakuEngine.Input.Events;
using DanmakuEngine.Input.Handlers;
using DanmakuEngine.Input.States;
using DanmakuEngine.Logging;

namespace DanmakuEngine.Input;

public class InputManager : UpdateOnlyObject
{
    public IInputHandler[] Handlers { get; private set; }

    public readonly MouseManager Mouse = new MouseManager();

    public readonly InputState State = new InputState();

    public InputManager()
    {
        Handlers =
        [
            Mouse.Initialize(new MouseHandler()),
            new KeyboardHandler(),
        ];
    }

    public void Register(GameHost host)
    {
        foreach (var h in Handlers)
        {
            Logger.Debug($"Registering {h.GetType()}");

            h.AutoInject();

            h.Register(host);
        }

        State.Initialize(host);
    }

    private Queue<IInputEvent> _events = new Queue<IInputEvent>();
    protected override void Update()
    {
        _events.Clear();

        foreach (var h in Handlers)
        {
            h.GetEvents(ref _events);
        }

        // TODO: Handle events collected from input devices.
    }
}
