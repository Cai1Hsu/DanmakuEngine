using DanmakuEngine.Arguments;
using DanmakuEngine.Engine;
using DanmakuEngine.Logging;
using DanmakuEngine.Tests.Games;
using DanmakuEngine.Timing;
using NUnit.Framework;

namespace DanmakuEngine.Tests.Timing;

public class TestTime
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
    public void TestDeltaTime()
    {
        var testGame = new TestGame();

        using var host = new TestGameHost(5000)
        {
            SkipFirstFrame = true,
        };

        host.OnUpdate += h =>
        {
            Assert.That(Time.UpdateDelta, Is.GreaterThan(0));

            Assert.That(Time.ElapsedSeconds, Is.GreaterThan(0));

            h.RequestClose();
        };

        host.OnTimedout += Assert.Fail;

        host.Run(testGame, defaultProvider);
    }

    [Test]
    public void TestTimeReset()
    {
        // Since the defaultProvider will be disposed after config loadingm, we should declare an extra provider.
        var argProvider1 = ArgumentProvider.CreateDefault(new ParamTemplate(), []);
        var argProvider2 = ArgumentProvider.CreateDefault(new ParamTemplate(), []);

        var testGame = new TestGame();

        using var host1 = new TestGameHost(0)
        {
            IgnoreTimedout = true,
            ThrowOnTimedOut = false,
        };

        host1.Run(testGame, argProvider1);

        using var host2 = new TestGameHost(5000);

        host2.OnLoad += h =>
        {
            Assert.That(Time.ElapsedSeconds, Is.LessThan(0.5));

            h.RequestClose();
        };

        host2.Run(testGame, argProvider2);
    }

    [Test]
    public void TestTestGameHostUpdateFrames()
    {
        var testGame = new TestGame();

        int count_frame = 0;

        using var host = new TestGameHost(100)
        {
            // Fix error output in CI
            ThrowOnTimedOut = false,
            IgnoreTimedout = true
        };

        host.OnUpdate += h =>
        {
            count_frame++;
        };

        host.Run(testGame, defaultProvider);

        Assert.That(count_frame, Is.GreaterThan(1));
    }

    [Test]
    public void TestTimeAlwaysGoForward()
    {
        double lastClockElapsedSeconds = 0;
        double lastFixedElapsedSeconds = 0;
        double lastElapsedSeconds = 0;
        double lastEngineElapsedSeconds = 0;

        Clock clock = new();

        var game = new TestGame();

        using var host = new TestGameHost(100);

        host.OnUpdate += h =>
        {
            Assert.That(clock.ElapsedSeconds, Is.GreaterThan(lastClockElapsedSeconds).Within(1E-6));
            Assert.That(Time.FixedElapsedSeconds, Is.GreaterThanOrEqualTo(lastFixedElapsedSeconds).Within(1E-6));
            Assert.That(Time.ElapsedSeconds, Is.GreaterThanOrEqualTo(lastElapsedSeconds).Within(1E-6));
            Assert.That(Time.EngineTimer.ElapsedSeconds, Is.GreaterThanOrEqualTo(lastEngineElapsedSeconds).Within(1E-6));

            Assert.That(clock.DeltaTime, Is.Not.Negative);
            Assert.That(Time.UpdateDelta, Is.Not.Negative);

            lastClockElapsedSeconds = clock.ElapsedSeconds;
            lastFixedElapsedSeconds = Time.FixedElapsedSeconds;
            lastElapsedSeconds = Time.ElapsedSeconds;
            lastEngineElapsedSeconds = Time.EngineTimer.ElapsedSeconds;
        };

        host.OnLoad += _ => clock.Start();

        host.Run(game, defaultProvider);
    }

    [Test]
    public void TestElapsedSecondsConsistentWithDelta()
    {
        var game = new TestGame();

        using var host = new TestGameHost(1000)
        {
            BypassThrottle = true,
            IgnoreTimedout = true,
            ThrowOnTimedOut = false,
        };

        double accumulatedSeconds = 0;

        host.OnUpdate += h =>
        {
            accumulatedSeconds += Time.UpdateDelta;
            Logger.Debug($"    Accumulated seconds: {accumulatedSeconds * 1000:F2}, Elapsed seconds: {Time.ElapsedSeconds * 1000:F2}");
            Assert.That(accumulatedSeconds, Is.EqualTo(Time.ElapsedSeconds).Within(1E-6));
        };

        host.Run(game, defaultProvider);
    }
}
