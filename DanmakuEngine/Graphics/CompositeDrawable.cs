using Silk.NET.Maths;

namespace DanmakuEngine.Graphics;

public class CompositeDrawable : Drawable
{
    public Vector2D<float> Size = new(0, 0);

    private readonly List<Drawable> children = new();

    public CompositeDrawable(CompositeDrawable parent) : base(parent)
    {
    }

    public IReadOnlyList<Drawable> Children
        => children.AsReadOnly();

    public void Add(Drawable child)
        => children.Add(child);

    public override bool UpdateSubTree()
    {
        if (base.UpdateSubTree())
            return true;

        if (children.Any(c => c.UpdateSubTree()))
            return true;

        // TODO: Something else?

        return false;
    }

    public override void Update()
    {
        
    }
}