using DanmakuEngine.Dependency;
using DanmakuEngine.Graphics;
using DanmakuEngine.Input.Handlers;
using DanmakuEngine.Logging;
using DanmakuEngine.Timing;

namespace DanmakuEngine.Games.Screens;

public class Screen : CompositeDrawable, IInjectable
{
    public override ScreenStack Parent => (ScreenStack)base.Parent;

    protected ScreenStack ScreenStack => Parent;

    public IKeyboardHandler keyboardHandler = null!;

    public ScreenClock Clock { get; } = new();

    public Screen(ScreenStack parent) : base(parent)
    {
        // TODO: Should we have this?
        this.load();
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
        Clock.Update(Time.UpdateDelta);

        Update();
    }

    public override void Update()
    {

    }

    public void start()
    {
        Clock.Reset();

        Start();
    }

    public override void Start()
    {

    }

    public new void load()
    {
        base.load();

        DependencyContainer.AutoInject(this);

        Load();
    }

    public override void Load()
    {

    }
}