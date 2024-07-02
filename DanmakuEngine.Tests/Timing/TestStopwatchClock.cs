using System.Diagnostics;
using DanmakuEngine.Timing;
using NUnit.Framework;

namespace DanmakuEngine.Tests.Timing;

public class TestStopwatchClock
{
    private double sleep(double seconds)
    {
        var sw = Stopwatch.StartNew();

        Thread.Sleep((int)(seconds * 1000));

        return sw.Elapsed.TotalSeconds;
    }

    [Test]
    public void StartNew_ShouldInitializeAndStartClock()
    {
        var clock = StopwatchClock.StartNew();
        Assert.That(clock.IsPaused, Is.False);

        clock = new StopwatchClock();

        Assert.That(clock.IsPaused, Is.True);

        clock.Start();

        Assert.That(clock.IsPaused, Is.False);
    }

    [Test]
    public void Pause_ShouldPauseClock()
    {
        var clock = StopwatchClock.StartNew();
        clock.Pause();
        Assert.That(clock.IsPaused, Is.True);
    }

    [Test]
    public void Resume_ShouldResumeClock()
    {
        var clock = StopwatchClock.StartNew();
        clock.Pause();
        clock.Resume();
        Assert.That(clock.IsPaused, Is.False);
    }

    public void Start_ShouldHaveElapsedSeconds()
    {
        var clock = new StopwatchClock();
        Assert.That(clock.ElapsedSeconds, Is.EqualTo(0));
        clock.Start();

        var slept = sleep(0.1);

        Assert.That(clock.ElapsedSeconds, Is.EqualTo(slept).Within(0.01));
    }

    [Test]
    public void Reset_ShouldResetClock()
    {
        var clock = StopwatchClock.StartNew();
        sleep(0.05);

        clock.Reset();

        Assert.That(clock.ElapsedSeconds, Is.EqualTo(0));
        Assert.That(clock.IsPaused, Is.True);
    }

    [Test]
    public void StepIn_ShouldAddSecondsToClock()
    {
        var clock = new StopwatchClock();

        var current_time = clock.ElapsedSeconds;

        clock.StepIn(1);

        Assert.That(clock.ElapsedSeconds, Is.EqualTo(current_time + 1).Within(1E-6));
    }

    [Test]
    public void StepOut_ShouldDecreaseAccumulatedSeconds()
    {
        var clock = new StopwatchClock();

        var current_time = clock.ElapsedSeconds;

        clock.StepIn(10);
        clock.StepOut(5);

        Assert.That(clock.ElapsedSeconds, Is.EqualTo(current_time + 5).Within(1E-6));
    }

    [Test]
    public void StepOut_ShouldThrowExceptionWhenStepOutMoreThanAccumulated()
    {
        var clock = new StopwatchClock();
        clock.StepIn(5);

        Assert.Throws<ArgumentException>(() => clock.StepOut(10));
    }

    [Test]
    public void Playback_DefaultIs1()
    {
        var clock = new StopwatchClock();

        Assert.That(clock.Playback, Is.EqualTo(1));
    }

    public void Playback_ResetPlaybackTo1()
    {
        var clock = new StopwatchClock();
        clock.SetPlayback(2, false);

        clock.ResetPlayback();

        Assert.That(clock.Playback, Is.EqualTo(1));
    }

    public void Playback_ResetMethodDoesNotChangePlayback()
    {
        var clock = new StopwatchClock();
        clock.SetPlayback(2, false);

        clock.Reset();

        Assert.That(clock.Playback, Is.EqualTo(2));
    }

    [Test]
    public void SetPlayback_ShouldChangePlaybackProperty()
    {
        var clock = new StopwatchClock();

        clock.SetPlayback(2, false);

        Assert.That(clock.Playback, Is.EqualTo(2));
    }

    [Test]
    public void SetPlayback_ShouldThrowExceptionForZeroPlayback()
    {
        var clock = new StopwatchClock();
        Assert.Throws<ArgumentException>(() => clock.SetPlayback(0, false));
    }

    [Test]
    public void SetPlayback_ShouldThrowExceptionForNegativePlayback()
    {
        var clock = new StopwatchClock();
        Assert.Throws<ArgumentException>(() => clock.SetPlayback(-1, false));
    }

    [Test]
    public void Playback_ElapsedSecondsShouldChangeAccordingToPlayback()
    {
        var clock = new StopwatchClock();
        clock.SetPlayback(2, false);

        clock.Start();

        var slept = sleep(0.1);

        clock.Pause();

        Assert.That(clock.ElapsedSeconds, Is.EqualTo(slept * 2).Within(1E-3));
    }

    [Test]
    public void Playback_PauseShouldNotChangeElapsedSeconds()
    {
        var clock = new StopwatchClock();

        clock.Start();

        var slept = sleep(0.1);

        clock.Pause();

        clock.SetPlayback(2, false);

        Assert.That(clock.ElapsedSeconds, Is.EqualTo(slept).Within(1E-3));
    }
}
