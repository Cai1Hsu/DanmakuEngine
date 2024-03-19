using System.Runtime.CompilerServices;
using DanmakuEngine.Arguments;
using DanmakuEngine.Engine;
using DanmakuEngine.Tests.Games;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace DanmakuEngine.Tests;

public class TestGameHost : HeadlessGameHost
{
    private LinkedList<TestPoint> _testPoints = new();

    public TestGameHost(double timeout)
        : base(timeout)
    {
        ConfigManager.TestModeDectected();

        this.OnLoad += _ =>
        {
            this.OnUpdate += _ => executeTest();
        };
    }

    private void executeTest()
    {
        var node = _testPoints.First;

        while (node is not null)
        {
            var point = node.Value;

            if (point.Condition())
            {
                point.TestAction(this);

                if (point.Done)
                    _testPoints.Remove(node);
            }

            node = node.Next;
        }
    }

    public TestGameHost AddTestPoint(TestPoint point)
    {
        point.SetHost(this);

        _testPoints.AddLast(point);

        return this;
    }
}
