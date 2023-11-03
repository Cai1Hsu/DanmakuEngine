using DanmakuEngine.Dependency;

namespace DanmakuEngine.Games;

public class Game : IInjectable
{
    public readonly string Name = "Danmaku!";
    
    public Game()
    {

    }

    public void Inject(DependencyContainer container)
    {
        // throw new NotImplementedException();
    }
}