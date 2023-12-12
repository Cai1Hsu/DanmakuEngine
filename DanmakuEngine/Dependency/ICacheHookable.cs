namespace DanmakuEngine.Dependency;

public interface ICacheHookable
{
    public void OnCache(DependencyContainer dependencies);
}
