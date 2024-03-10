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

        using var host1 = new TestGameHost(5000);

        host1.OnUpdate += h =>
        {
            if (Time.ElapsedSeconds > 0.5)
                h.RequestClose();
        };

        host1.OnTimedout += Assert.Fail;

        host1.Run(testGame, argProvider1);

        using var host2 = new TestGameHost(5000);

        host2.OnUpdate += h =>
        {
            Assert.That(Time.ElapsedSeconds, Is.LessThan(0.5));

            h.RequestClose();
        };

        host2.OnTimedout += Assert.Fail;

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
}
