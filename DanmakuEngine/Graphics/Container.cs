using Silk.NET.Maths;

namespace DanmakuEngine.Graphics;

public class Container<T> : CompositeDrawable
{
    public Container(CompositeDrawable parent) : base(parent)
    {
    }
}

public class DrawableContainer : Container<Drawable>
{
    public DrawableContainer(CompositeDrawable parent) : base(parent)
    {
    }
}