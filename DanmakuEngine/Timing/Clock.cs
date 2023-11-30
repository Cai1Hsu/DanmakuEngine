namespace DanmakuEngine.Timing;

public class Clock
{
    /// <summary>
    /// Represents the current time of the clock in ms. 
    /// This time can be affected by <see cref="Playback"/> and <seealso cref="SetPlayback"/>
    /// </summary>
    public double CurrentTime => _accomulatedTime + (IsPaused ? 0 : realElpsedTime * Playback);

    private double realElpsedTime => Time.CurrentTime - _startTime;

    private double _startTime;

    private double _accomulatedTime = 0;

    private bool _isPaused = true;

    public bool IsPaused => _isPaused;

    private double _playback = 1.00;

    public double Playback => _playback;

    /// <summary>
    /// You may want to use this because the delta time is affected by the playback.
    /// This is helpful when you want to make a slow motion effect.
    /// </summary>
    public double UpdateDelta => IsPaused ? 0 : Time.UpdateDelta * _playback;

    /// <summary>
    /// see <see cref="UpdateDelta"/> for more info.
    /// </summary>
    public double RenderDelta => IsPaused ? 0 : Time.RenderDelta * _playback;

    public Clock(bool start = false)
    {
        Reset();

        if (start)
            Start();
        else
            Pause();
    }

    public void Reset()
    {
        _startTime = Time.CurrentTime;

        _accomulatedTime = 0;
    }

    /// <summary>
    /// Pause everything related to the clock
    /// For example, you can implement and update objects related to `Sakuya` with <see cref="Time"/>
    /// and everything elses with a managed Clock.
    /// </summary>
    public void Pause()
    {
        _accomulatedTime = this.CurrentTime;

        _isPaused = true;
    }

    public void Start()
    {
        Reset();

        _isPaused = false;
    }

    public void Resume()
    {
        _startTime = Time.CurrentTime;

        _isPaused = false;
    }

    public void ResetPlayback()
    {
        if (Playback == 1)
            return;

        SetPlayback(1.00, true);
    }

    public void SetPlayback(double playback, bool bypassCheck = false)
    {
        if (!bypassCheck)
        {
            if (playback == 0)
                throw new ArgumentException("Playback cannot be 0");

            if (playback < 0)
                throw new ArgumentException($"Playback must be positive, but found: {playback}");

            if (_playback == playback)
                return;
        }

        _playback = playback;

        if (IsPaused)
            return;

        _accomulatedTime = this.CurrentTime;
        _startTime = Time.CurrentTime;
    }
}