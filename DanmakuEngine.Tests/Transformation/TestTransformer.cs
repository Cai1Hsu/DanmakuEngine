using DanmakuEngine.Transformation;
using DanmakuEngine.Engine;
using DanmakuEngine.Tests.Games;
using DanmakuEngine.Bindables;
using DanmakuEngine.Arguments;

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
        var host = new TestGameHost(5000);

        double value = 0;
        double expeted = 0;

        var t = new TransformSequence(
                new Transformer(1000, new LinearIn(), p => value = p);
        );

        host.OnUpdate += _ =>
        {
            t.Update(Time.Clock.UpdateDelta * 1000);
            expected += Time.Clock.UpdateDelta;

            Assert.That(t.Value, Is.EqualTo(expected).Within(1E-6));
        };

        host.Run(testGame, defaultProvider);
    }

    [Test]
    public void TestWithBindable()
    {
        var host = new TestGameHost(5000);
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
