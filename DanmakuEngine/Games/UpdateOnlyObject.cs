namespace DanmakuEngine.Games;

public class UpdateOnlyObject : GameObject
{
    public UpdateOnlyObject() : base(LoadState.Complete)
    {
    }

    public override bool updateSubTree()
    {
        update();

        return false;
    }
}
