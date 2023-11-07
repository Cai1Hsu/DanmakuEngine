using Silk.NET.Maths;

namespace DanmakuEngine.Graphics;

public abstract class CompositeDrawable : Drawable
{
    public Vector2D<float> Size = new(0, 0);

    protected virtual ICollection<Drawable> Children { get; set; } = null!;

    public CompositeDrawable(CompositeDrawable parent) : base(parent)
    {
        this.Children = new LinkedList<Drawable>();
    }

    public void Add(Drawable child)
        => Children.Add(child);

    public override bool UpdateSubTree()
    {
        if (base.UpdateSubTree())
            return true;

        if (!Children.All(c => !c.UpdateSubTree()))
            return true;

        // TODO: Something else?

        return false;
    }

    public override void Update()
    {

    }
}