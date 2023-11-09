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

    public Clock Clock { get; } = new();

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