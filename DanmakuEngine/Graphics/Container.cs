using Silk.NET.Maths;

namespace DanmakuEngine.Graphics;

public class Container<T> : Drawable
{
    public Vector2D<float> Size = new(0, 0);

    private readonly List<T> children = new();

    public IReadOnlyList<T> Children
        => children.AsReadOnly();

    public void Add(T child)
        => children.Add(child);

    public Container()
    {

    }
}

public class DrawableContainer : Container<Drawable>
{

}