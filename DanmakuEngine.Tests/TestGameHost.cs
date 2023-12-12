using DanmakuEngine.Engine;

namespace DanmakuEngine.Tests;

public class TestGameHost : HeadlessGameHost
{
    private LinkedList<TestPoint> _testPoints = new();

    public long CurrentFrame { get; private set; } = 0;

    public TestGameHost(double timeout) : base(timeout)
    {
        this.OnLoad += _ =>
        {
            this.OnUpdate += _ => CurrentFrame++;
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
