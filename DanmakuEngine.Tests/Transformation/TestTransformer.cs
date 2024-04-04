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
        defaultProvider = ArgumentProvider.CreateDefault(new ParamTemplate(), Array.Empty<string>());
        testGame = new TestGame();
    }

    [TearDown]
    public void TearDown()
    {
        defaultProvider.Dispose();
    }

    [Test]
    public void TestWithOnUpdate()
    {
        using var host = new TestGameHost(150)
        {
            IgnoreTimedout = true,
            ThrowOnTimedOut = false,
        };

        double value = 0;
        double expected = 0;

        Transformer transformer;

        var t = new TransformSequence(
            transformer = new Transformer(100, new LinearIn(), p => value = p)
        );

        host.OnUpdate += h =>
        {
            t.Update(Time.UpdateDelta);
            expected += Time.UpdateDelta / 100;

            Assert.That(transformer.Value, Is.EqualTo(expected).Within(1E-6));
        };

        host.Run(testGame, defaultProvider);
    }

    [Test]
    public void TestWithBindable()
    {
        using var host = new TestGameHost(100)
        {
            IgnoreTimedout = true,
            ThrowOnTimedOut = false,
        };

        Bindable<double> bindable = new();

        var t = new Transformer(1000, new LinearIn()).BindTo(bindable);

        host.OnUpdate += _ =>
        {
            t.Update(Time.UpdateDelta * 1000);

            Assert.That(bindable.Value, Is.EqualTo(t.Value));
        };

        host.Run(testGame, defaultProvider);
    }
}
