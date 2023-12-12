using DanmakuEngine.Timing;

namespace DanmakuEngine.Movement;

public class LinearIncreaseMovement : DoubleMovement
{
    public LinearIncreaseMovement(double speed, IClock clock)
        : base(clock)
    {
        this.Speed.Value = speed;
    }
}
