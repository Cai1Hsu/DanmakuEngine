using DanmakuEngine.Engine;
using DanmakuEngine.Scheduling;
using DanmakuEngine.Timing;
using DanmakuEngine.Arguments;
using DanmakuEngine.Tests.Games;
using NUnit.Framework;

namespace DanmakuEngine.Tests.Scheduling;

public class TestScheduler
{
    private ArgumentProvider defaultProvider = null!;

    [SetUp]
    public void SetUp()
    {
        defaultProvider = ArgumentProvider.CreateDefaultProvider(new ParamTemplate(), Array.Empty<string>());
    }

    [Test]
    public void TestScheduleTask()
    {
        var testGame = new TestGame();

        using var host = new HeadlessGameHost(5000);

        var clock = new Clock();

        var scheduler = new Scheduler(clock);

        host.OnLoad += h =>
        {
            clock.Start();
            scheduler.ScheduleTask(() => h.RequestClose());
        };

        host.OnUpdate += h => scheduler.update();

        host.OnTimedout += Assert.Fail;

        host.Run(testGame, defaultProvider);
    }

    [Test]
    public void TestScheduleTaskDelay()
    {
        const double test_seconds = 0.1;

        var testGame = new TestGame();

        using var host = new HeadlessGameHost(5000);

        var clock = new Clock(true);

        var scheduler = new Scheduler(clock);

        double current_time = 0;

        host.OnLoad += h =>
        {
            // We didn't start the clock on construction, because the `Time` was not cleared yet
            clock.Start();

            current_time = clock.CurrentTime;

            scheduler.ScheduleTaskDelay(() => h.RequestClose(), test_seconds);
        };

        host.OnUpdate += _ =>
        {
            scheduler.update();
            current_time = Math.Max(current_time, clock.CurrentTime);
        };

        host.OnTimedout += Assert.Fail;

        host.Run(testGame, defaultProvider);

        Assert.That(current_time, Is.GreaterThanOrEqualTo(test_seconds));
    }
}