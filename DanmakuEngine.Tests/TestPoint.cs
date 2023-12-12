using DanmakuEngine.Tests.Games;

namespace DanmakuEngine.Tests;

public class TestPoint
{
    public TestGameHost Host { get; private set; } = null!;

    public Func<bool> Condition { get; private set; } = null!;

    public Action<TestGameHost> TestAction { get; private set; } = null!;

    public void SetHost(TestGameHost host)
        => this.Host = host;

    private bool _executed = false;

    private int _execute_count = -1;

    private int _current_count = 0;

    public bool Done => _executed && _current_count < _execute_count;

    public TestPoint SetInfinite()
    {
        _execute_count = -1;

        return this;
    }

    public TestPoint(Action<TestGameHost> testAction, int count)
        : this(testAction, () => true, count)
    {
    }

    public TestPoint(Action<TestGameHost> testAction)
        : this(testAction, () => true)
    {
    }

    public TestPoint(Action<TestGameHost> testAction, Func<bool> condition)
        : this(testAction, condition, 1)
    {
    }

    public TestPoint(Action<TestGameHost> testAction, Func<bool> condition, int count)
    {
        this.Condition = condition;
        this.TestAction = testAction;

        this.TestAction += _ =>
        {
            _execute_count++;
            _executed = true;
        };

        this._execute_count = count;
    }
}
