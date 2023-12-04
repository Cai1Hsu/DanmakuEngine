using System.Diagnostics;
using DanmakuEngine.Dependency;
using DanmakuEngine.Graphics;
using DanmakuEngine.Input.Handlers;
using DanmakuEngine.Timing;

namespace DanmakuEngine.Games.Screens;

public partial class Screen : CompositeDrawable
{
    private ScreenStack _parent = null!;

    public override ScreenStack Parent => _parent;

    protected ScreenStack ScreenStack => Parent;

    public UserKeyboardHandler keyboardHandler = null!;

    /// <summary>
    /// The clock for the screen
    /// it provides standard time for the screen
    /// be careful when pause the clock or change the rate as it may cause unexpected behavior
    /// </summary>
    public Clock ScreenClock { get; } = new();

    protected List<IUpdatable> InternalChildren { get; set; } = null!;

    public void SetParent(ScreenStack parent)
    {
        if (_parent is not null)
            return;

        _parent = parent;
    }

    public Screen() : base(null!)
    {
        if (this is IInjectable injectable)
            injectable.AutoInject();

        // in Drawable.load which is called in the update loop
        // this.load();
    }

    public bool updateSubTree()
    {
        // FIXME: this doesn't work properly
        // it always return true
        if (base.UpdateSubTree())
            return true;

        // TODO: update children

        return UpdateSubTree();
    }

    // TODO: Whether we should have this
    public override bool UpdateSubTree()
    {
        return false;
    }

    public override void update()
    {
        base.update();

        if (InternalChildren is not null)
        {
            foreach (var ichild in InternalChildren)
                ichild.update();
        }
    }

    // this is called in Drawable.update()
    public override void Update()
    {

    }

    protected override void start()
    {
        ScreenClock.Reset();
        ScreenClock.Start();

        base.start();

        if (InternalChildren is not null)
        {
            foreach (var ichild in InternalChildren)
                ichild.start();
        }
    }

    public override void Start()
    {

    }

    public override void load()
    {
        // we make the Screen base class injectable
        if (this is IInjectable injectable)
            injectable.AutoInject();

        // we dont have to assert as early as in the constructor
        Debug.Assert(_parent != null, $"ScreenStack for {GetType()} is null");

        base.load();

        if (InternalChildren is not null)
        {
            foreach (var ichild in InternalChildren)
                ichild.load();
        }
    }

    public override void Load()
    {

    }
}
