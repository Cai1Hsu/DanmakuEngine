namespace DanmakuEngine.Timing;

public interface IHasPlayback
{
    public double Playback => 1;

    public void SetPlayback(double playback, bool bypassCheck);

    public void ResetPlayback();
}
