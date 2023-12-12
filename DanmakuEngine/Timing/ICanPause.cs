namespace DanmakuEngine.Timing;

public interface ICanPause
{
    public bool IsPaused { get; }

    public void Start();

    public void Stop();

    public void Pause();

    public void Reset();

    public void Resume();
}
