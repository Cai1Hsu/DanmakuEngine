using System.Diagnostics;
using DanmakuEngine.Arguments;
using DanmakuEngine.Engine;
using DanmakuEngine.Logging;
using DanmakuEngine.Movements;
using DanmakuEngine.Tests.Games;
using DanmakuEngine.Timing;
using NUnit.Framework;

namespace DanmakuEngine.Tests.Movements;

public class TestFixedAcceleratedMovement
{
    private ArgumentProvider defaultProvider;

    [SetUp]
    public void SetUp()
    {
        defaultProvider = ArgumentProvider.CreateDefault(new ParamTemplate(), Array.Empty<string>());
    }

    [TearDown]
    public void TearDown()

    {
        defaultProvider.Dispose();
    }

    [Test]
    public void TestMove()
    {
        const double time = 1;
        const double acceleration = 1;

        var game = new TestGame();

        var host = new TestGameHost(500)
        {
            ThrowOnTimedOut = false,
            IgnoreTimedout = true,
        };

        var clock = new Clock();

        var movement = new FixedAcceleratedMovementD(0, acceleration)
        {
            Condition = _ => clock.ElapsedSeconds < time,
        };

        host.OnLoad += h =>
        {
            clock.Start();
            movement.BeginMove();
        };

        host.OnUpdate += _ =>
        {
            movement.UpdateSubTree();

            var expected = 0.5 * acceleration * (clock.ElapsedSeconds * clock.ElapsedSeconds);

            Assert.That(movement.Value.Value, Is.EqualTo(expected).Within(1E-6));
        };

        host.Run(game, defaultProvider);
    }

    [Test]
    public void TestStop()
    {
        const double time = 0.1;
        const double acceleration = 1;

        var game = new TestGame();

        var host = new TestGameHost(500);

        var movement = new FixedAcceleratedMovementD(0, acceleration)
        {
            Condition = _ => Time.ElapsedSeconds < time,
            OnDone = _ =>
            {
                host.RequestClose();
            },
        };

        host.OnLoad += _ =>
        {
            movement.BeginMove();
        };

        host.OnUpdate += _ =>
        {
            movement.UpdateSubTree();
        };

        host.OnTimedout += () =>
        {
            Assert.Fail($"OnDone is not called.");
        };

        host.Run(game, defaultProvider);

        Assert.That(movement.IsDone, Is.True);
    }

    [Test]
    public void TestMoveManyFrames()
    {
        const double acceleration = 1;
        var clock = new Clock();

        var game = new TestGame();
        var host = new TestGameHost(1000)
        {
            ThrowOnTimedOut = false,
            IgnoreTimedout = true,
        };

        var movement = new FixedAcceleratedMovementD(0, acceleration);

        host.OnLoad += _ =>
        {
            clock.Start();
            movement.BeginMove();
        };

        host.OnUpdate += _ =>
        {
            movement.UpdateSubTree();

            // x = 0.5 * a * t^2
            var expected = 0.5 * acceleration * (clock.ElapsedSeconds * clock.ElapsedSeconds);

            Assert.That(movement.Value.Value, Is.EqualTo(expected).Within(1E-6));
        };

        int count_frame = 0;

        host.OnUpdate += _ => count_frame++;

        host.Run(game, defaultProvider);
    }

    [Test]
    public void TestMoveWithRandomLargeDelta()
    {
        const double acceleration = 1;

        var clock = new TestClock();

        var movement = new FixedAcceleratedMovementD(0, acceleration);

        movement.SetClock(clock)
                .BeginMove();

        int count_frame = 0;

        var rng = new Random();

        do
        {
            var random_delta = rng.NextDouble() * 100;
            clock.SetDeltaTime(random_delta);
            clock.AccomulateTime();

            movement.UpdateSubTree();

            var expected = 0.5 * acceleration * (clock.ElapsedSeconds * clock.ElapsedSeconds);

            Assert.That(movement.Value.Value, Is.EqualTo(expected).Within(1E-6));
        } while (count_frame++ < 1E6);
    }

    private class TestClock : IClock
    {
        private double updateDelta = 0;

        private double currentTime;

        public double DeltaTime => updateDelta;

        // We dont use this here
        public double RenderDelta => 0;

        public double ElapsedSeconds
            => currentTime;

        public bool IsPaused => false;

        public void SetDeltaTime(double delta)
        {
            updateDelta = delta;
        }

        public void AccomulateTime()
        {
            currentTime += updateDelta;
        }
    }
}
