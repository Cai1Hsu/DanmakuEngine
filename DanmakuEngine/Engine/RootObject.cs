using System.Diagnostics;
using System.Reflection;
using DanmakuEngine.Graphics;
using DanmakuEngine.Timing;

namespace DanmakuEngine.Engine;

public class RootObject : DrawableContainer
{
    public RootObject()
        : base(null!)
    {
        Debug.Assert(LoadState is Games.LoadState.NotLoaded);
    }

    protected override void update()
    {
        // Implementation for FixedUpdate.
        int count = 0;

        while (Time.FixedElapsedSeconds + Time.FixedUpdateDelta < Time.ElapsedSeconds
            && count < 5) // never allow FixedUpdate blocks the game logic too heavily
        {
            Time.FixedElapsedSeconds += Time.FixedUpdateDelta;

            count++;

            FixedUpdateSubtree();
        }

        base.update();
    }

    protected override void Start()
    {
        base.Start();
    }
}
