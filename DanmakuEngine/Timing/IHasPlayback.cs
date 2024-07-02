namespace DanmakuEngine.Timing;

public interface IHasPlayback
{
    public double Playback { get; }

    public void SetPlayback(double playback, bool bypassCheck);

    public void ResetPlayback();
}
