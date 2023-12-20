namespace DanmakuEngine.Games;

public class UpdateOnlyObject : GameObject
{
    public UpdateOnlyObject() : base(LoadState.Complete)
    {
    }

    public override bool UpdateSubTree()
    {
        update();

        return false;
    }
}
