using DanmakuEngine.Dependency;
using DanmakuEngine.Games.Screens;
using Silk.NET.Input;

namespace DanmakuEngine.Input.Handlers;

public class TopKeyboardHandler : IInputHandler
{
    public TopKeyboardHandler()
    {

    }

    [Inject]
    private ScreenStack _screens = null!;

    [Inject]
    private IInputContext _input = null!;

    public void OnLoad()
    {
        foreach (var k in _input.Keyboards)
        {
            k.KeyDown += KeyDown;
            k.KeyUp += KeyUp;
        }
    }

    public void KeyDown(IKeyboard arg1, Key arg2, int arg3)
    {
        _screens.Peek().keyboardHandler.KeyDown(arg1, arg2, arg3);
    }

    public void KeyUp(IKeyboard arg1, Key arg2, int arg3)
    {
        _screens.Peek().keyboardHandler.KeyUp(arg1, arg2, arg3);
    }
}