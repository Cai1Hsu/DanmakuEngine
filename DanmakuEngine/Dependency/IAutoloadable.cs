namespace DanmakuEngine.Dependency;

public interface IAutoloadable : IInjectable
{
    /// <summary>
    /// This method is called when auto injection to your class finished.
    /// Usually you can strat your work here.
    /// </summary>
    public void OnLoad();
}
