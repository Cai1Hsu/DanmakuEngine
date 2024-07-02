using System.Diagnostics;

namespace DanmakuEngine.Timing;

public class StopwatchClock : IClock, ICanStep, IHasPlayback, ICanPause
{
    private readonly Stopwatch _source = new();

    public Stopwatch Source => _source;

    private double _accomulatedSeconds = 0;
    private TimePeriod _timePeriod = TimePeriod.NewPeriod(0);

    public bool IsPaused => !_source.IsRunning;

    public double ElapsedMilliseconds => ElapsedSeconds * 1000;

    public double ElapsedSeconds
        => IsPaused ? _accomulatedSeconds : _accomulatedSeconds + (_timePeriod.Elapsed(sourceElapsedSeconds) * _playback);

    public double realElapsedTime => IsPaused ? _accomulatedSeconds : _accomulatedSeconds + _timePeriod.Elapsed(sourceElapsedSeconds);

    public double DeltaTime => IsPaused ? 0 : realDeltaTime * _playback;

    private double realDeltaTime => realElapsedTime - lastElapsedSeconds;

    private double sourceElapsedSeconds => _source.ElapsedTicks / (double)Stopwatch.Frequency;

    public static StopwatchClock StartNew()
    {
        var clock = new StopwatchClock();
        clock.Start();
        return clock;
    }

    private double lastElapsedSeconds = 0;

    public void Update()
    {
        if (IsPaused)
            return;

        lastElapsedSeconds = realElapsedTime;
    }

    public void Start()
    {
        if (_source.IsRunning)
            _source.Stop();

        _source.Start();

        _timePeriod.New(0);
    }

    private void newPeriod()
    {
        _accomulatedSeconds = this.ElapsedSeconds;
        _timePeriod.New(0);
    }

    public void Reset()
    {
        _source.Stop();
        _source.Reset();
        _accomulatedSeconds = 0;
        _timePeriod.New(0);
    }

    public void Pause()
    {
        newPeriod();
        _source.Stop();
        _source.Reset();
    }

    public void Resume()
    {
        if (_source.IsRunning)
            return;

        _source.Start();
    }

    public void StepIn(double seconds)
    {
        _accomulatedSeconds += seconds;
    }

    public void StepOut(double seconds)
    {
        if (_accomulatedSeconds < seconds)
        {
            // let's try accomulate time
            // and see if it's enough to step out
            _accomulatedSeconds = this.ElapsedSeconds;

            newPeriod();
            _source.Restart();

            if (_accomulatedSeconds < seconds)
                throw new ArgumentException($"Cannot step out more than the accomulated time, accomulated time: {_accomulatedSeconds}, step out time: {seconds}.");
        }

        _accomulatedSeconds -= seconds;
    }

    private double _playback = 1;

    public double Playback
    {
        get => _playback;
        set => SetPlayback(value, false);
    }

    public void SetPlayback(double playback, bool bypassCheck)
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

        _source.Stop();

        newPeriod();

        _playback = playback;

        _source.Restart();
    }

    public void ResetPlayback()
    {
        if (Playback == 1)
            return;

        SetPlayback(1.00, true);
    }

    public void Stop()
    {
        _source.Stop();
    }
}
