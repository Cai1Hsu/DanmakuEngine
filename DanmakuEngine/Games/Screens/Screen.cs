using System.Diagnostics;
using DanmakuEngine.Dependency;
using DanmakuEngine.Graphics;
using DanmakuEngine.Timing;

namespace DanmakuEngine.Games.Screens;

public partial class Screen : CompositeDrawable
{
    private ScreenStack _parent = null!;

    public override ScreenStack Parent => _parent;

    protected ScreenStack ScreenStack => Parent;

    /// <summary>
    /// The clock for the screen
    /// it provides standard time for the screen
    /// be careful when pause the clock or change the rate as it may cause unexpected behavior
    /// </summary>
    public Clock ScreenClock => Clock;

    public void SetParent(ScreenStack parent)
    {
        if (_parent is not null)
            return;

        _parent = parent;
    }

    public Screen() : base(null!)
    {
    }

    public override bool UpdateSubTree()
    {
        if (base.UpdateSubTree())
            return true;

        var shouldStop = false;

        // update children
        if (InternalChildren is not null)
        {
            foreach (var ichild in InternalChildren)
                shouldStop |= ichild.UpdateSubTree();
        }

        return shouldStop;
    }

    protected override void start()
    {
        ScreenClock.Start();

        base.start();
    }

    protected override void load()
    {
        // we dont have to assert as early as in the constructor
        Debug.Assert(_parent != null, $"ScreenStack for {GetType()} is null");

        // Create our ScreenClock so that we can use it later
        var _ = Clock;

        base.load();
    }

    public virtual void OnScreenLeaving()
    {
    }

    public virtual void OnScreenEntering()
    {
    }
}
