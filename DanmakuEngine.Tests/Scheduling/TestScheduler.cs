using DanmakuEngine.Arguments;
using DanmakuEngine.Engine;
using DanmakuEngine.Scheduling;
using DanmakuEngine.Tests.Games;
using DanmakuEngine.Timing;
using NUnit.Framework;

namespace DanmakuEngine.Tests.Scheduling;

public class TestScheduler
{
    private ArgumentProvider defaultProvider = null!;

    [SetUp]
    public void SetUp()
    {
        defaultProvider = ArgumentProvider.CreateDefault(new ParamTemplate(), Array.Empty<string>());
    }

    [Test]
    public void TestScheduleTask()
    {
        var testGame = new TestGame();

        using var host = new TestGameHost(5000);

        var scheduler = new Scheduler();

        host.OnLoad += h =>
        {
            scheduler.ScheduleTask(() => h.RequestClose());
        };

        host.OnUpdate += h => scheduler.UpdateSubTree();

        host.OnTimedout += Assert.Fail;

        host.Run(testGame, defaultProvider);
    }

    [Test]
    public void TestScheduleTaskDelay()
    {
        const double test_seconds = 0.1;

        var testGame = new TestGame();

        using var host = new TestGameHost(5000);

        var scheduler = new Scheduler();

        double current_time = 0;

        host.OnLoad += h =>
        {
            current_time = Time.ElapsedSeconds;

            scheduler.ScheduleTaskDelay(() => h.RequestClose(), test_seconds);
        };

        host.OnUpdate += _ =>
        {
            scheduler.UpdateSubTree();
            current_time = Math.Max(current_time, Time.ElapsedSeconds);
        };

        host.OnTimedout += Assert.Fail;

        host.Run(testGame, defaultProvider);

        Assert.That(current_time, Is.GreaterThanOrEqualTo(test_seconds));
    }
}
