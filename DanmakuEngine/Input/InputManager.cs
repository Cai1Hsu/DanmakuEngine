using DanmakuEngine.Dependency;
using Silk.NET.Input;
using Silk.NET.Windowing;

namespace DanmakuEngine.Input;

public class InputManager : IInjectable, ICacheHookable
{
    private IInputContext Input { get; }
    [Inject]
    private IWindow _window = null!;

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
            {
                _window.Close();
            }
        }
    }

    public void Inject(DependencyContainer container)
    {
        this._window = container.Get<IWindow>();
    }

    public void OnCache(DependencyContainer container)
    {
        container.Cache(Input);
    }
}
