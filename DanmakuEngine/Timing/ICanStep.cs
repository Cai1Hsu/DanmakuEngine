namespace DanmakuEngine.Timing;

public interface ICanStep
{
    public void StepIn(double seconds);

    public void StepOut(double seconds);
}
