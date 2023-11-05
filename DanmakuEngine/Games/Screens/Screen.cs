using DanmakuEngine.Dependency;
using DanmakuEngine.Graphics;
using DanmakuEngine.Input.Handlers;
using DanmakuEngine.Timing;

namespace DanmakuEngine.Games.Screens;

public class Screen : CompositeDrawable, IInjectable
{
    public override ScreenStack Parent => (ScreenStack) base.Parent;

    public IKeyboardHandler keyboardHandler = null!;

    public ScreenClock Clock { get; } = new();

    public Screen(ScreenStack parent) : base(parent)
    {
    }

    public override bool UpdateSubTree()
    {
        if (Parent.Peek() != this)
            return true;

        if (base.UpdateSubTree())
            return true;

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

    public void load()
    {
        DependencyContainer.AutoInject(this);

        Load();
    }

    public override void Load()
    {
        
    }
}