using DanmakuEngine.Arguments;
using DanmakuEngine.Bindables;
using DanmakuEngine.Engine;
using DanmakuEngine.Tests.Games;
using DanmakuEngine.Timing;
using DanmakuEngine.Transformation;
using DanmakuEngine.Transformation.Functions;
using NUnit.Framework;

namespace DanmakuEngine.Tests.Transfomrmation;

public class TestTransformer
{
    private ArgumentProvider defaultProvider;
    private TestGame testGame;

    [SetUp]
    public void SetUp()
    {
        defaultProvider = ArgumentProvider.CreateDefaultProvider(new ParamTemplate(), Array.Empty<string>());
        testGame = new TestGame();
    }

    [Test]
    public void TestWithOnUpdate()
    {
        var host = new TestGameHost(1500)
        {
            BypassWaitForSync = true,
            IgnoreTimedout = true,
            ThrowOnTimedOut = false,
        };

        double value = 0;
        double expected = 0;

        Transformer transformer;

        var t = new TransformSequence(
            transformer = new Transformer(1000, new LinearIn(), p => value = p)
        );

        host.OnUpdate += h =>
        {
            t.Update(Time.Clock.UpdateDelta * 1000);
            expected += Time.Clock.UpdateDelta;

            if (expected > 1)
            {
                h.RequestClose();

                return;
            }

            Assert.That(transformer.Value, Is.EqualTo(expected).Within(1E-6));
        };

        host.Run(testGame, defaultProvider);
    }

    [Test]
    public void TestWithBindable()
    {
        var host = new TestGameHost(100)
        {
            IgnoreTimedout = true,
            ThrowOnTimedOut = false,
            BypassWaitForSync = true,
        };

        Bindable<double> bindable = new();

        var t = new Transformer(1000, new LinearIn()).BindTo(bindable);

        host.OnUpdate += _ =>
        {
            t.Update(Time.Clock.UpdateDelta * 1000);

            Assert.That(bindable.Value, Is.EqualTo(t.Value));
        };

        host.Run(testGame, defaultProvider);
    }
}
