namespace DanmakuEngine.Timing;

public class Clock : IClock, ICanStep, IHasPlayback, ICanTheWorld
{
    #region IClock
    /// <summary>
    /// Represents the current time of the clock in seconds. 
    /// This time can be affected by <see cref="Playback"/> and <seealso cref="SetPlayback"/>
    /// </summary>
    public double CurrentTime => _accomulatedTime + (IsPaused ? 0 : currentPeriodElapsedTime);

    private double realElapsedTime => Time.CurrentTime - _startTime;

    private double currentPeriodElapsedTime
    {
        get
        {
            if (_state is ClockState.TheWorld)
            {
                if (!isTheWorldFinished())
                    return 0;

                finishTheWorld();
            }

            return realElapsedTime * Playback;
        }
    }

    private double _startTime;

    private double _accomulatedTime = 0;

    public bool IsPaused
    {
        get
        {
            if (isTheWorldFinished())
                finishTheWorld();

            return _state is ClockState.Paused or ClockState.TheWorld;
        }
    }

    /// <summary>
    /// You may want to use this because the delta time is affected by the playback.
    /// This is helpful when you want to make a slow motion effect.
    /// 
    /// In seconds.
    /// </summary>
    public double UpdateDelta
    {
        get
        {
            if (isTheWorldFinished())
                finishTheWorld();

            return IsPaused ? 0 : Time.UpdateDelta * _playback;
        }
    }

    /// <summary>
    /// see <see cref="UpdateDelta"/> for more info.
    /// 
    /// In seconds.
    /// </summary>
    public double RenderDelta
    {
        get
        {
            if (isTheWorldFinished())
                finishTheWorld();

            return IsPaused ? 0 : Time.RenderDelta * _playback;
        }
    }

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

        _state = ClockState.Paused;
    }

    /// <summary>
    /// Pause everything related to the clock
    /// For example, you can implement and update objects related to `Sakuya` with <see cref="Time"/>
    /// and everything elses with a managed Clock.
    /// </summary>
    public void Pause()
    {
        _accomulatedTime = this.CurrentTime;

        _state = ClockState.Paused;
    }

    public void Start()
    {
        Reset();

        _state = ClockState.Running;
    }

    public void Resume()
    {
        _startTime = Time.CurrentTime;

        _state = ClockState.Running;
    }

    public void Stop()
    {
        this.Pause();
        this.Reset();
    }

    #endregion

    #region Playback

    private double _playback = 1.00;

    public double Playback => _playback;

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

        // accomulate time before _playback was changed.
        // it changes CurrentTime calculation
        _accomulatedTime = this.CurrentTime;

        _playback = playback;

        if (IsPaused)
            return;

        _startTime = Time.CurrentTime;
    }

    #endregion

    #region StepIn/StepOut

    /// <summary>
    /// Step in the clock by a specific time.
    /// </summary>
    /// <param name="seconds"> The time to step in in seconds. </param>
    public void StepIn(double seconds)
        => _accomulatedTime += seconds;

    /// <summary>
    /// Step out the clock by a specific time.
    /// This will throw an exception if the result is negative.
    /// </summary>
    /// <param name="seconds"> The time to step out in seconds. </param> 
    /// <exception cref="ArgumentException"> If the result is negative. </exception> 
    public void StepOut(double seconds)
    {
        if (_accomulatedTime < seconds)
        {
            // let's try accomulate time
            // and see if it's enough to step out
            _accomulatedTime = this.CurrentTime;
            _startTime = Time.CurrentTime;

            if (_accomulatedTime < seconds)
                throw new ArgumentException($"Cannot step out more than the accomulated time, accomulated time: {_accomulatedTime}, step out time: {seconds}.");
        }

        _accomulatedTime -= seconds;
    }

    #endregion

    #region TheWorld

    private double _theworldStartTime = 0;
    private double _theworldTime = 0;

    /// <summary>
    /// Pause the clock for a specific time.
    /// </summary>
    /// <param name="time">time in seconds</param>
    /// <returns>True if the clock was paused, false otherwise</returns>
    public bool TheWorld(double seconds)
    {
        // TODO:Handle paused cases
        if (this.IsPaused)
            return false;

        _theworldStartTime = Time.CurrentTime;
        _theworldTime = seconds;

        Pause();

        _state = ClockState.TheWorld;

        return true;
    }

    private void finishTheWorld()
    {
        Resume();

        _startTime = Time.CurrentTime;

        // accomulate the extra time
        _accomulatedTime += ((Time.CurrentTime - _theworldStartTime) - _theworldTime) * Playback;
    }

    private bool isTheWorldFinished()
    {
        if (_state is not ClockState.TheWorld)
            return false;

        return Time.CurrentTime - _theworldStartTime >= _theworldTime;
    }
    #endregion

    private ClockState _state = ClockState.Paused;

    private enum ClockState
    {
        Running,
        Paused,
        TheWorld
    }
}
