namespace DanmakuEngine.Dependency;

public interface IInjectable
{
    public void Inject(DependencyContainer container);
}