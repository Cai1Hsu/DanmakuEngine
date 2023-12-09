using Silk.NET.Maths;

namespace DanmakuEngine.Graphics;

public abstract class CompositeDrawable : Drawable
{
    public Vector2D<float> Size = new(0, 0);

    protected virtual IEnumerable<Drawable> Children => children;

    protected virtual LinkedList<Drawable> children { get; set; } = null!;

    public CompositeDrawable(CompositeDrawable parent) : base(parent)
    {
        this.children = new LinkedList<Drawable>();
    }

    public void Add(Drawable child)
        => children.AddLast(child);

    /// <summary>
    /// Updates the drawable and all of its children
    /// </summary>
    /// <returns>false if continues to update, and true for stops</returns>
    public override bool updateSubTree()
    {
        if (base.updateSubTree())
            return true;

        if (UpdateChildren())
            return true;

        return false;
    }

    protected virtual bool UpdateChildren()
    {
        bool shouldStop = false;

        foreach (var child in Children)
            shouldStop |= child.updateSubTree();

        return shouldStop;
    }
}