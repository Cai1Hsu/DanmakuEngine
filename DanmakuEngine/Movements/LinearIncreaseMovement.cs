using DanmakuEngine.Timing;

namespace DanmakuEngine.Movement;

public class LinearIncreaseMovement : DoubleMovement
{
    public LinearIncreaseMovement(double speed, Clock clock)
        : base(clock)
    {
        this.Speed = speed;
    }
}