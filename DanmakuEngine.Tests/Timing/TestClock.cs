using DanmakuEngine.Arguments;
using DanmakuEngine.Engine;
using DanmakuEngine.Logging;
using DanmakuEngine.Timing;
using NUnit.Framework;

namespace DanmakuEngine.Tests.Timing;

public class TestClock
{
    private ArgumentProvider defaultProvider;

    [SetUp]
    public void SetUp()
    {
        defaultProvider = ArgumentProvider.CreateDefaultProvider(new ParamTemplate(), Array.Empty<string>());
    }

    [Test]
    public void TestUpdateClock()
    {
        Clock clock = new();

        var game = new TestGame();

        using var host = new HeadlessGameHost(5000);

        host.OnUpdate += h =>
        {
            Assert.That(clock.UpdateDelta, Is.GreaterThan(0));
            Assert.That(clock.RenderDelta, Is.GreaterThan(0));

            Assert.That(clock.CurrentTime, Is.GreaterThan(0));

            h.RequestClose();
        };

        host.OnLoad += _ =>
        {
            clock.Start();
            clock.Reset();
        };

        host.Run(game, defaultProvider);
    }

    [Test]
    public void TestPause()
    {
        Clock clock = new();

        int count_frame = 0;
        double current_time = 0;

        var game = new TestGame();

        using var host = new HeadlessGameHost(5000);

        host.OnUpdate += h =>
        {
            count_frame += 1;

            if (count_frame > 60)
                h.RequestClose();

            if (current_time == 0)
                current_time = clock.CurrentTime;
            else
                Assert.That(clock.CurrentTime, Is.EqualTo(current_time));
        };

        host.OnLoad += _ => clock.Start();

        host.Run(game, defaultProvider);
    }

    [Test]
    public void TestResume()
    {
        Clock clock = new();

        int count_frame = 0;
        double current_time = 0;

        var game = new TestGame();

        using var host = new HeadlessGameHost(5000);

        host.OnUpdate += h =>
        {
            count_frame += 1;

            if (count_frame < 60)
            {
                if (current_time == 0)
                    current_time = clock.CurrentTime;
                else
                    Assert.That(clock.CurrentTime, Is.EqualTo(current_time));
            }
            else if (count_frame < 120)
            {
                clock.Resume();
            }
            else if (count_frame < 180)
            {
                Assert.That(clock.CurrentTime, Is.GreaterThan(current_time));
            }
            else
            {
                h.RequestClose();
            }
        };

        host.OnLoad += _ => clock.Start();

        host.Run(game, defaultProvider);
    }

    [Test]
    public void TestPlayback()
    {
        Clock clock = new();

        var game = new TestGame();

        using var host = new HeadlessGameHost(5000);

        host.OnUpdate += h =>
        {
            Assert.That(clock.UpdateDelta, Is.EqualTo(Time.UpdateDelta * 2).Within(0.002));
            Assert.That(clock.CurrentTime, Is.EqualTo(Time.CurrentTime * 2).Within(0.002));

            h.RequestClose();
        };

        host.OnLoad += _ =>
        {
            clock.SetPlayback(2.0);

            clock.Start();
        };

        host.Run(game, defaultProvider);
    }

    [Test]
    public void TestSetPlaybackMultipleTimes()
    {
        Clock clock = new();

        int count_frame = 0;

        double current_time = 0;

        var game = new TestGame();

        using var host = new HeadlessGameHost(5000);

        host.OnUpdate += h =>
        {
            count_frame += 1;

            if (count_frame == 30)
            {
                current_time = Time.CurrentTime;

                clock.SetPlayback(2.0);
            }
            else if (count_frame == 60)
            {
                // half of the second part and the first part
                var correct_clock_time = (Time.CurrentTime - current_time) / 2 + current_time;

                Assert.That(clock.CurrentTime, Is.EqualTo(correct_clock_time).Within(0.002));

                h.RequestClose();
            }
        };

        host.OnLoad += _ => clock.Start();

        host.Run(game, defaultProvider);
    }

    [Test]
    public void TestResetPlayback()
    {
        Clock clock = new();

        var game = new TestGame();

        using var host = new HeadlessGameHost(5000);

        host.OnUpdate += h =>
        {
            clock.ResetPlayback();

            Assert.That(clock.Playback, Is.EqualTo(1));

            h.RequestClose();
        };

        host.OnLoad += _ =>
        {
            clock.SetPlayback(2.0);

            clock.Start();
        };

        host.Run(game, defaultProvider);
    }

    [Test]
    public void TestResetClock()
    {
        Clock clock = new();

        var game = new TestGame();

        using var host = new HeadlessGameHost(5000);

        host.OnUpdate += h =>
        {
            clock.Reset();

            Assert.That(clock.CurrentTime, Is.EqualTo(0));

            h.RequestClose();
        };

        host.OnLoad += _ => clock.Start();

        host.Run(game, defaultProvider);
    }
}