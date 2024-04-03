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
}
