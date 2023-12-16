using DanmakuEngine.Allocations;

namespace DanmakuEngine.Timing;

public class Time
{
    /// <summary>
    /// Represents and controls the update frequency of the fps in debug console
    /// it's the delay seconds between updating the fps in debug console.
    /// You shouldn't set a very low value.
    /// </summary>
    protected double UpdateFrequency = 1;
    protected double fps_debug_time = 0;

    protected double count_time = 0;
    protected int count_frame = 0;

    public static double AverageFramerate { get; private set; }

    public static double Jitter { get; private set; }

    private RingPool<double> _delta_pool = new(128);

    protected virtual void UpdateTime()
    {
        CurrentTime += UpdateDelta;

        count_time += UpdateDelta;
        count_frame++;

        fps_debug_time += UpdateDelta;

        _delta_pool.CurrentAndNext = UpdateDelta;

        double avg = _delta_pool.Get().Average();
        Jitter = Math.Sqrt(_delta_pool.Get().Average(v => Math.Pow(v - avg, 2)));

        if (count_time < 1)
            return;

        AverageFramerate = count_frame / count_time;

        count_frame = 0;
        count_time = 0;
    }

    /// <summary>
    /// Current time of the game main loop in seconds
    /// </summary>
    public static double CurrentTime { get; protected set; }

    /// <summary>
    /// Render delta of the last frame in seconds
    /// </summary>
    public static double RenderDelta { get; protected set; } = 1 / 60;

    /// <summary>
    /// Update delta of the last frame in seconds
    /// </summary>
    public static double UpdateDelta { get; protected set; } = 1 / 60;

    public static readonly StandardClock Clock = new();

    public void ResetTime(double refresh_rate)
    {
        // Having referred code from many other game engines
        // I believe we should make initial delta time to be 0

        // reference 1 (Overload):
        //     in: Sources/Overload/OvTools/src/OvTools/Time/Clock.cpp
        //
        // void OvTools::Time::Clock::Initialize()
        // {
        //     __DELTA_TIME = 0.0f;
        //
        //     __START_TIME = std::chrono::steady_clock::now();
        //     __CURRENT_TIME = __START_TIME;
        //     __LAST_TIME = __START_TIME;
        //
        //     __INITIALIZED = true;
        // }

        // reference 2 (osu-framework):
        //     in: Timing/FramedClock.cs
        //
        // public double ElapsedFrameTime => CurrentTime - LastFrameTime;
        //
        //     and
        //
        // public void ChangeSource(IClock source)
        // {
        //     Source = source;
        //     CurrentTime = LastFrameTime = source.CurrentTime;
        // }

        // reference 3 (UE):
        // However, the delta time is not 0 in UE.
        // They set it to an *Optimistic default* value, which is used in our previous version.
        //
        // Personally, i do not agree with this design. Since Besides, we are going to separate the update thread and render thread
        // and our game can update at a very fast rate, which can be up to thousands of frames per second and far beyond the refresh rate of the monitor and can not be predicted.
        // But in UE, they update and render synchronously, and the update rate is always the same as the render rate.
        // And you can definitely not run a UE game at a very high framerate even if u have a very powerful computer.
        // So i think it's reasonable to set the delta time to 0 in our game engine. But i leave it here for reference.
        //
        //     in: Engine/Source/Runtime/TimeManagement/Private/GenlockedCustomTimeStep.cpp
        //
        // void UGenlockedCustomTimeStep::UpdateAppTimes(const double& TimeBeforeSync, const double& TimeAfterSync) const
        // {
        //     // Use fixed delta time to update FApp times.
        //
        //     double ActualDeltaTime;
        //     {
        //         // Multiply sync time by valid SyncCountDelta to know ActualDeltaTime
        //         if (IsLastSyncDataValid() && (GetLastSyncCountDelta() > 0))
        //         {
        //             ActualDeltaTime = GetLastSyncCountDelta() * GetSyncRate().AsInterval();
        //         }
        //         else
        //         {
        //             // optimistic default
        //             ActualDeltaTime = GetFixedFrameRate().AsInterval();
        //         }
        //     }
        //
        //     FApp::SetCurrentTime(TimeAfterSync);
        //     FApp::SetIdleTime((TimeAfterSync - TimeBeforeSync) - (ActualDeltaTime - GetFixedFrameRate().AsInterval()));
        //     FApp::SetDeltaTime(ActualDeltaTime);
        // }

        UpdateDelta = 0;
        RenderDelta = 0;

        CurrentTime = 0;
    }
}
