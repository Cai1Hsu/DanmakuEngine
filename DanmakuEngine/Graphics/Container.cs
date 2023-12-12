using Silk.NET.Maths;

namespace DanmakuEngine.Graphics;

public class DrawableContainer : CompositeDrawable
{
    public DrawableContainer(CompositeDrawable parent) : base(parent)
    {
    }
}
