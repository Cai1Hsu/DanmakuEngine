namespace DanmakuEngine.Games;

public class UpdateOnlyObject : GameObject
{
    public UpdateOnlyObject() : base(LoadState.Complete)
    {
    }

    public override bool UpdateSubTree(bool fixedUpdate = false)
    {
        if (!fixedUpdate)
            update();
        else
            base.fixedUpdate();

        return false;
    }
}
