using DanmakuEngine.Input.Handlers;
using DanmakuEngine.Dependency;
using DanmakuEngine.Games;
using Silk.NET.Input;

namespace DanmakuEngine.Input;

public class InputManager : ICacheHookable
{
    private IInputContext Input { get; }

    public List<IInputHandler> Handlers { get; private set; } = null!;

    public InputManager(IInputContext input)
    {
        this.Input = input;
    }
    
    public void OnCache(DependencyContainer dependencies)
    {
        dependencies.Cache(Input);
        
        Handlers = new List<IInputHandler>()
        {
            new TopKeyboardHandler(),
        };

        foreach (var h in Handlers)
            h.AutoInject();
    }
}
