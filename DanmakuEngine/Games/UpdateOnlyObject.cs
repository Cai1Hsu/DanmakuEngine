using Silk.NET.Vulkan;

namespace DanmakuEngine.Games;

public class UpdateOnlyObject : GameObject
{
    public UpdateOnlyObject()
        : base(LoadState.Complete)
    {
    }

    protected sealed override void load()
    {
    }

    protected sealed override void Load()
    {
    }

    protected sealed override void start()
    {
    }

    protected sealed override void Start()
    {
    }

    public override bool UpdateSubTree()
    {
        update();

        return false;
    }
}
