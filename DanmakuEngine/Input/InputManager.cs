using DanmakuEngine.Dependency;
using DanmakuEngine.Games;
using Silk.NET.Input;
using Silk.NET.Windowing;

namespace DanmakuEngine.Input;

public class InputManager : IInjectable, ICacheHookable
{
    private IInputContext Input { get; }

    [Inject]
    private GameHost _host = null!;

    public InputManager(IInputContext input)
    {
        this.Input = input;

        foreach (var t in Input.Keyboards)
        {
            t.KeyDown += KeyDown;
        }

        void KeyDown(IKeyboard arg1, Key arg2, int arg3)
        {
            //Check to close the window on escape.
            if (arg2 == Key.Escape)
                _host.window.IsClosing = true;
        }
    }

    public void Inject(DependencyContainer container)
    {
        _host = container.Get<GameHost>();
    }

    public void OnCache(DependencyContainer container)
    {
        container.Cache(Input);
    }
}
