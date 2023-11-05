namespace DanmakuEngine.Games.Screens;

public class ScreenClock
{
    public double CurrentTime { get; private set; }

    public void Reset()
    {
        CurrentTime = 0;
    }

    public void Update(double delta)
    {
        CurrentTime += delta;
    }
}