using DanmakuEngine.Dependency;
using Silk.NET.Input;
using Silk.NET.Windowing;

namespace DanmakuEngine.Input;

public class InputManager : IInjectable
{
    private IInputContext Input { get; }
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
        container.Cache(Input);
        this._window = container.Get<IWindow>();
    }
}
