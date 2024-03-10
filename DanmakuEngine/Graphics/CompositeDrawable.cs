using DanmakuEngine.Games;
using Silk.NET.Maths;

namespace DanmakuEngine.Graphics;

public abstract class CompositeDrawable : Drawable
{
    public Vector2D<float> Size = new(0, 0);

    public virtual List<GameObject> Children
    {
        get => children ??= new List<GameObject>();
        set
        {
            if (children is null)
                children = value.ToList();
            else
                throw new InvalidOperationException("Children is already assigned");
        }
    }

    protected virtual List<GameObject> children { get; set; } = null!;

    public CompositeDrawable(CompositeDrawable parent)
        : base(parent)
    {
    }

    public void Add(GameObject child)
        => Children.Add(child);

    public void Remove(GameObject child)
        => Children.Remove(child);

    public void Add(params GameObject[] children)
    {
        foreach (var child in children)
            Add(child);
    }

    public void Remove(params GameObject[] children)
    {
        foreach (var child in children)
            Remove(child);
    }

    /// <summary>
    /// Updates the drawable and all of its children
    /// </summary>
    /// <returns>false if continues to update, and true for stops</returns>
    public override bool UpdateSubTree()
    {
        if (base.UpdateSubTree())
            return true;

        if (UpdateChildren())
            return true;

        return false;
    }

    protected virtual bool UpdateChildren()
    {
        bool shouldStop = false;

        foreach (var child in Children)
            shouldStop |= child.UpdateSubTree();

        return shouldStop;
    }

    public override void FixedUpdateSubtree()
    {
        base.FixedUpdateSubtree();

        FixedUpdateChildren();
    }

    protected virtual void FixedUpdateChildren()
    {
        if (children is not null)
        {
            foreach (var child in children)
                child.FixedUpdateSubtree();
        }
    }
}
