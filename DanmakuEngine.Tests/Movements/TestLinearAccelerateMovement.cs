using System.Diagnostics;
using DanmakuEngine.Arguments;
using DanmakuEngine.Engine;
using DanmakuEngine.Logging;
using DanmakuEngine.Movement;
using DanmakuEngine.Tests.Games;
using DanmakuEngine.Timing;
using NUnit.Framework;

namespace DanmakuEngine.Tests.Movements;

public class TestLinearAccelerateMovement
{
    private ArgumentProvider defaultProvider;

    [SetUp]
    public void SetUp()
    {
        defaultProvider = ArgumentProvider.CreateDefaultProvider(new ParamTemplate(), Array.Empty<string>());
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void TestMove(bool vsync)
    {
        const double time = 1;
        const double acceleration = 1;

        var game = new TestGame();

        var host = new HeadlessGameHost(500)
        {
            BypassWaitForSync = vsync,
            ThrowOnTimedOut = false,
            IgnoreTimedout = true,
        };

        var clock = new Clock();

        var movement = new LinearAccelerateMovement(acceleration, clock)
        {
            Condition = () => clock.CurrentTime < time,
        };

        host.OnLoad += h =>
        {
            clock.Start();
            movement.BeginMove();
        };

        host.OnUpdate += _ =>
        {
            movement.update();

            var expected = 0.5 * acceleration * (clock.CurrentTime * clock.CurrentTime);

            Assert.That(movement.Value, Is.EqualTo(expected).Within(1E-6));
        };

        host.Run(game, defaultProvider);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void TestStop(bool vsync)
    {
        const double time = 0.1;
        const double acceleration = 1;

        var game = new TestGame();

        var host = new HeadlessGameHost(500)
        {
            BypassWaitForSync = vsync,
        };

        var clock = new Clock();

        var movement = new LinearAccelerateMovement(acceleration, clock)
        {
            Condition = () => clock.CurrentTime < time,
            OnDone = () => host.RequestClose(),
        };

        host.OnLoad += h =>
        {
            clock.Start();
            movement.BeginMove();
        };

        host.OnUpdate += _ =>
        {
            movement.update();
        };

        host.OnTimedout += () =>
        {
            Assert.Fail("OnDone is not called");
        };

        host.Run(game, defaultProvider);

        Assert.That(movement.Done, Is.True);
    }

    [Test]
    public void TestMoveManyFrames()
    {
        const double acceleration = 1;
        var clock = new Clock();

        var game = new TestGame();
        var host = new HeadlessGameHost(1000)
        {
            BypassWaitForSync = true,
            ThrowOnTimedOut = false,
            IgnoreTimedout = true,
        };

        var movement = new LinearAccelerateMovement(acceleration, clock);

        host.OnLoad += _ =>
        {
            clock.Start();
            movement.BeginMove();
        };

        host.OnUpdate += _ =>
        {
            movement.update();

            // x = 0.5 * a * t^2
            var expected = 0.5 * acceleration * (clock.CurrentTime * clock.CurrentTime);

            Assert.That(movement.Value, Is.EqualTo(expected).Within(1E-6));
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

        var movement = new LinearAccelerateMovement(acceleration, clock);

        movement.BeginMove();

        int count_frame = 0;

        do
        {
            var random_delta = new Random().NextDouble() * 100;
            clock.SetUpdateDelta(random_delta);
            clock.AccomulateTime();

            movement.update();

            var expected = 0.5 * acceleration * (clock.CurrentTime * clock.CurrentTime);

            Assert.That(movement.Value, Is.EqualTo(expected).Within(1E-6));
        } while (count_frame++ < 1E6);
    }

    private class TestClock : IClock
    {
        private double updateDelta = 0;

        private double currentTime;

        public double UpdateDelta => updateDelta;

        // We dont use this here
        public double RenderDelta => 0;

        public double CurrentTime
            => currentTime;

        public bool IsPaused => false;

        public void SetUpdateDelta(double delta)
        {
            updateDelta = delta;
        }

        public void AccomulateTime()
        {
            currentTime += updateDelta;
        }
    }
}