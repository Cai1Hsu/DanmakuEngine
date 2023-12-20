using System.Diagnostics;
using DanmakuEngine.Dependency;
using DanmakuEngine.Graphics;
using DanmakuEngine.Input.Handlers;
using DanmakuEngine.Input.Keybards;
using DanmakuEngine.Timing;

namespace DanmakuEngine.Games.Screens;

public partial class Screen : CompositeDrawable
{
    private ScreenStack _parent = null!;

    public override ScreenStack Parent => _parent;

    protected ScreenStack ScreenStack => Parent;

    public KeyboardHandler keyboardHandler = null!;

    /// <summary>
    /// The clock for the screen
    /// it provides standard time for the screen
    /// be careful when pause the clock or change the rate as it may cause unexpected behavior
    /// </summary>
    public Clock ScreenClock => Clock;

    protected List<GameObject> InternalChildren { get; set; } = null!;

    public void SetParent(ScreenStack parent)
    {
        if (_parent is not null)
            return;

        _parent = parent;
    }

    public Screen() : base(null!)
    {
        // in Drawable.load which is called in the update loop
        // this.load();
    }

    public override bool updateSubTree()
    {
        if (base.updateSubTree())
            return true;

        var shouldStop = false;

        // update children
        if (InternalChildren is not null)
        {
            foreach (var ichild in InternalChildren)
                shouldStop |= ichild.updateSubTree();
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
        // we make the Screen base class injectable
        if (this is IInjectable injectable)
            injectable.AutoInject();

        // we dont have to assert as early as in the constructor
        Debug.Assert(_parent != null, $"ScreenStack for {GetType()} is null");

        // Create our ScreenClock so that we can use it later
        var _ = Clock;

        base.load();

        keyboardHandler?.RegisterKeys();
    }
}
