using DanmakuEngine.Arguments;
using DanmakuEngine.Engine;
using DanmakuEngine.Logging;
using DanmakuEngine.Timing;
using NUnit.Framework;

namespace DanmakuEngine.Tests.Timing;

public class TestTime
{
    private ArgumentProvider defaultProvider;
    [SetUp]
    public void SetUp()
    {
        defaultProvider = ArgumentProvider.CreateDefaultProvider(new ParamTemplate(), Array.Empty<string>());
    }

    [Test]
    public void TestUpdateDelta()
    {
        var testGame = new TestGame();

        using var host = new HeadlessGameHost(5000);

        host.OnUpdate += h =>
        {
            Assert.That(Time.UpdateDelta, Is.GreaterThan(0));

            Assert.That(Time.CurrentTime, Is.GreaterThan(0));

            h.RequestClose();
        };

        host.OnTimedout += Assert.Fail;

        host.Run(testGame, defaultProvider);
    }


    [Test]
    public void TestTimeReset()
    {
        var testGame = new TestGame();

        using var host1 = new HeadlessGameHost(5000);

        host1.OnUpdate += h =>
        {
            if (Time.CurrentTime > 2000)
                h.RequestClose();
        };

        host1.OnTimedout += Assert.Fail;

        host1.Run(testGame, defaultProvider);

        using var host2 = new HeadlessGameHost(5000);

        host2.OnUpdate += h =>
        {
            Assert.That(Time.CurrentTime, Is.LessThan(2000));

            h.RequestClose();
        };

        host2.OnTimedout += Assert.Fail;

        host2.Run(testGame, defaultProvider);
    }
}