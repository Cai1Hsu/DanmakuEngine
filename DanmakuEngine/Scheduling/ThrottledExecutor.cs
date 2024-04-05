// from osu.Framework
// Copyright (c) ppy Pty Ltd <contact@ppy.sh>.

using System.Diagnostics;
using DanmakuEngine.Bindables;
using DanmakuEngine.Engine.Sleeping;
using DanmakuEngine.Logging;
using DanmakuEngine.Timing;

namespace DanmakuEngine.Scheduling;

public class ThrottledExecutor : FrameExecutor
{
    public readonly Bindable<bool> Active = new(true);

    public bool Throttling { get; set; } = true;

    internal double ActiveHz { get; set; } = 1000;

    internal double BackgroundHz { get; set; } = 60;

    public double Hz => Active.Value ? ActiveHz : BackgroundHz;

    public double SleptMilliseconds { get; private set; }

    public ThrottledExecutor(Action task)
        : base(task)
    {
    }

    public override void RunFrame()
    {
        base.RunFrame();

        if (Throttling)
        {
            if (Hz > 0 && Hz < double.MaxValue)
            {
                throttle();
            }
            else
            {
                // Even when running at unlimited frame-rate, we should call the scheduler
                // to give lower-priority background processes a chance to do work.
                SleptMilliseconds = sleepAndUpdateCurrent(0);
            }
        }
        else
        {
            SleptMilliseconds = 0;
        }
    }

    protected virtual double excessFrameTime => (1000.0 / Hz) - (DeltaTime * 1000);

    private double accumulatedSleepError;

    private void throttle()
    {
        double excessFrameTime = (1000.0 / Hz) - (DeltaTime * 1000);

        SleptMilliseconds = sleepAndUpdateCurrent(Math.Max(0, excessFrameTime + accumulatedSleepError));

        accumulatedSleepError += excessFrameTime - SleptMilliseconds;

        // Never allow the sleep error to become too negative and induce too many catch-up frames
        accumulatedSleepError = Math.Max(-1000 / 30.0, accumulatedSleepError);
    }

    private double sleepAndUpdateCurrent(double milliseconds)
    {
        // By returning here, in cases where the game is not keeping up, we don't yield.
        // Not 100% sure if we want to do this, but let's give it a try.
        if (milliseconds <= 0)
            return 0;

        double before = CurrentTime;

        WaitHandler.Wait(milliseconds);

        return ((CurrentTime = SourceTime) - before) * 1000;
    }
}
