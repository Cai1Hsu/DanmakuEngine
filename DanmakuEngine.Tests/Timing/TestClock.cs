using DanmakuEngine.Arguments;
using DanmakuEngine.Engine;
using DanmakuEngine.Graphics;
using DanmakuEngine.Logging;
using DanmakuEngine.Tests.Games;
using DanmakuEngine.Timing;
using NUnit.Framework;

namespace DanmakuEngine.Tests.Timing;

public class TestClock
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
    public void TestClockConsitentWithTime()
    {
        Clock clock = new();

        var game = new TestGame();

        using var host = new TestGameHost(100);

        host.OnUpdate += h =>
        {
            Assert.That(clock.ElapsedSeconds, Is.EqualTo(Time.ElapsedSeconds));

            Assert.That(clock.DeltaTime, Is.EqualTo(Time.UpdateDelta));
        };

        host.OnLoad += _ =>
        {
            clock.Start();
        };

        host.Run(game, defaultProvider);
    }

    [Test]
    public void TestUpdateClock()
    {
        Clock clock = new();

        var game = new TestGame();

        using var host = new TestGameHost(5000)
        {
            SkipFirstFrame = true,
        };

        host.OnUpdate += h =>
        {
            Assert.That(clock.DeltaTime, Is.GreaterThan(0));

            Assert.That(clock.ElapsedSeconds, Is.GreaterThan(0));

            h.RequestClose();
        };

        host.OnLoad += _ =>
        {
            clock.Start();
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

        using var host = new TestGameHost(5000);

        host.OnUpdate += h =>
        {
            count_frame += 1;

            if (count_frame == 1)
            {
                current_time = clock.ElapsedSeconds;

                clock.Pause();
            }

            Assert.That(clock.ElapsedSeconds, Is.EqualTo(current_time));

            if (count_frame > 60)
                h.RequestClose();
        };

        host.OnLoad += _ => clock.Start();

        host.Run(game, defaultProvider);
    }

    [Test]
    public void TestResume()
    {
        Clock clock = new();

        var game = new TestGame();

        using var host = new TestGameHost(500);

        ManualResetEventSlim tpLock = new(true);
        host.OnUpdate += _ =>
        {
            // Wait for thread pool work to finish
            tpLock.Wait();
        };

        host.OnLoad += h =>
        {
            clock.Start();

            Task.Run(async () =>
            {
                await Task.Delay(10);

                clock.Pause();

                var current_time = clock.ElapsedSeconds;

                await Task.Delay(50);

                tpLock.Reset();
                {
                    Assert.That(clock.ElapsedSeconds, Is.EqualTo(current_time).Within(1E-6));
                    clock.Resume();
                    Assert.That(clock.ElapsedSeconds, Is.EqualTo(current_time).Within(1E-6));
                }
                tpLock.Set();

                await Task.Delay(10);

                tpLock.Reset();
                {
                    Assert.That(clock.ElapsedSeconds, Is.GreaterThan(current_time).Within(1E-6));
                }
                tpLock.Set();

                h.RequestClose();
            });
        };

        host.Run(game, defaultProvider);
    }

    [Test]
    public void TestPlayback()
    {
        Clock clock = new();

        var game = new TestGame();

        using var host = new TestGameHost(5000);

        host.OnUpdate += h =>
        {
            Assert.That(clock.DeltaTime, Is.EqualTo(Time.UpdateDelta * 2).Within(0.001));
            Assert.That(clock.ElapsedSeconds, Is.EqualTo(Time.ElapsedSeconds * 2).Within(0.001));

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

        using var host = new TestGameHost(5000);

        host.OnUpdate += h =>
        {
            count_frame += 1;

            if (count_frame == 30)
            {
                current_time = Time.ElapsedSeconds;

                clock.SetPlayback(2.0);
            }
            else if (count_frame == 60)
            {
                // half of the second part and the first part
                var correct_clock_time = (Time.ElapsedSeconds - current_time) * 2 + current_time;

                Assert.That(clock.ElapsedSeconds, Is.EqualTo(correct_clock_time).Within(0.001));

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

        using var host = new TestGameHost(5000);

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

        using var host = new TestGameHost(5000);

        host.OnUpdate += h =>
        {
            clock.Reset();

            Assert.That(clock.ElapsedSeconds, Is.EqualTo(0));

            h.RequestClose();
        };

        host.OnLoad += _ => clock.Start();

        host.Run(game, defaultProvider);
    }

    [Test]
    public void TestStepIn()
    {
        Clock clock = new();

        var game = new TestGame();

        using var host = new TestGameHost(5000);

        host.OnUpdate += h =>
        {
            var current_time = clock.ElapsedSeconds;

            clock.StepIn(1);

            Assert.That(clock.ElapsedSeconds, Is.EqualTo(current_time + 1).Within(1E-6));

            h.RequestClose();
        };

        host.OnLoad += _ => clock.Start();

        host.Run(game, defaultProvider);
    }

    [Test]
    public void TestStepOut()
    {
        Clock clock = new();

        var game = new TestGame();

        using var host = new TestGameHost(5000);

        host.OnUpdate += h =>
        {
            if (Time.ElapsedSeconds > 0.2)
            {
                var current_time = clock.ElapsedSeconds;

                clock.StepOut(0.1);

                Assert.That(clock.ElapsedSeconds, Is.EqualTo(current_time - 0.1));

                h.RequestClose();
            }
        };

        host.OnLoad += _ => clock.Start();

        host.Run(game, defaultProvider);
    }

    [Test]
    public void TestTheWorld()
    {
        Clock clock = new();

        var game = new TestGame();

        using var host = new TestGameHost(5000);

        bool checkpoint1 = true;
        double checkpoint1_time = -1;

        bool checkpoint2 = true;

        bool checkpoint3 = true;

        host.OnUpdate += h =>
        {
            if (!checkpoint1 && !checkpoint2 && checkpoint3 && Time.ElapsedSeconds > 0.7)
            {
                checkpoint3 = false;

                Assert.That(clock.IsPaused, Is.False);

                Assert.That(clock.ElapsedSeconds, Is.EqualTo(Time.ElapsedSeconds - 0.5).Within(1E-10));

                Assert.That(clock.DeltaTime, Is.EqualTo(Time.UpdateDelta));

                h.RequestClose();
            }

            if (!checkpoint1 && checkpoint2 && Time.ElapsedSeconds < 0.2)
            {
                checkpoint2 = false;

                Assert.That(clock.IsPaused, Is.True);

                Assert.That(clock.ElapsedSeconds, Is.EqualTo(checkpoint1_time));

                Assert.That(clock.DeltaTime, Is.EqualTo(0));
            }

            if (checkpoint1 && Time.ElapsedSeconds > 0.05)
            {
                checkpoint1 = false;

                checkpoint1_time = clock.ElapsedSeconds;

                clock.TheWorld(0.5);

                Assert.That(clock.IsPaused, Is.True);

                Assert.That(clock.ElapsedSeconds, Is.EqualTo(checkpoint1_time));

                Assert.That(clock.DeltaTime, Is.EqualTo(0));
            }
        };

        host.OnLoad += _ => clock.Start();

        host.Run(game, defaultProvider);
    }
}
