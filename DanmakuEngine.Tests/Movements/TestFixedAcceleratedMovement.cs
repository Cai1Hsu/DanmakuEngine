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

        using var host = new TestGameHost(100)
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

        using var host = new TestGameHost(500);

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
        using var host = new TestGameHost(100)
        {
            ThrowOnTimedOut = false,
            IgnoreTimedout = true,
            BypassThrottle = true,
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

        bool assertPass = true;

        do
        {
            var random_delta = rng.NextDouble() * 100;
            clock.Update(random_delta);

            movement.UpdateSubTree();

            var expected = 0.5 * acceleration * (clock.ElapsedSeconds * clock.ElapsedSeconds);

            assertPass &= Math.Abs(movement.Value.Value - expected) < 1E-6;
        } while (count_frame++ < 1E6);

        Assert.That(assertPass, Is.True);
    }

    private class TestClock : IClock
    {
        private double updateDelta = 0;

        private double currentTime;

        public double DeltaTime => updateDelta;

        public double ElapsedSeconds
            => currentTime;

        public bool IsPaused => false;

        public void Update(double delta)
        {
            updateDelta = delta;
            currentTime += updateDelta;
        }
    }
}
