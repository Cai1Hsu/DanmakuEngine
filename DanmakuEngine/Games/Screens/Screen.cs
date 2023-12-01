using System.Diagnostics;
using DanmakuEngine.Dependency;
using DanmakuEngine.Graphics;
using DanmakuEngine.Input.Handlers;
using DanmakuEngine.Timing;

namespace DanmakuEngine.Games.Screens;

public partial class Screen : CompositeDrawable, IInjectable
{
    [Inject]
    private ScreenStack _parent = null!;

    public override ScreenStack Parent => _parent;

    protected ScreenStack ScreenStack => Parent;

    public UserKeyboardHandler keyboardHandler = null!;

    public Clock ScreenClock { get; } = new();

    public Screen() : base(null!)
    {
        this.AutoInject();

        this.load();

        Debug.Assert(_parent != null, $"ScreenStack for {GetType()} is null");
    }

    public bool updateSubTree()
    {
        // FIXME: this doesn't work properly
        // it always return true
        if (base.UpdateSubTree())
            return true;

        update();

        // TODO: update children

        return UpdateSubTree();
    }

    // TODO: Whether we should have this
    public override bool UpdateSubTree()
    {
        return false;
    }

    public void update()
    {
        base.Update();
    }

    public override void Update()
    {

    }

    protected override void start()
    {
        ScreenClock.Reset();
        ScreenClock.Start();

        base.start();
    }

    public override void Start()
    {

    }

    public new void load()
    {
        DependencyContainer.AutoInject(this);

        base.load();
    }

    public override void Load()
    {

    }
}